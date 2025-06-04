using System;
using System.Diagnostics;
using MTV3D65;

namespace Keystone
{
    public class DeviceCaps
    {
        private int _currentAdapter = 0;
        private string[][] _displayModes;
        private string[] _adapters;
        private int _adapterCount;

        public string[] DisplayModes
        {
            get { return _displayModes[_currentAdapter]; }
        }

        public string[] AvailableAdapters
        {
            get { return _adapters; }
        }


        public string CurrentAdapterName
        {
            get { return _adapters[_currentAdapter]; }
            set
            {
                for (int i = 0; i < _adapterCount; i++)
                {
                    if (value == _adapters[i])
                    {
                        CurrentAdapter = i;
                        return;
                    }
                }
                // the adapter is invalid so, if the previous adapter also is no good, 
                // then lets try to select adapter 0.  Adapter 0 should always exist or else windows wont even run right?
                if (_currentAdapter > _adapterCount - 1)
                    CurrentAdapter = 0;
            }
        }

        public int CurrentAdapter
        {
            get { return _currentAdapter; }
            set
            {
                if (value > _adapterCount - 1) throw new ArgumentOutOfRangeException();
                _currentAdapter = value;
            }
        }

        public DeviceCaps()
        {
            TVDeviceInfo info = new TVDeviceInfo();

            TV_MODEFORMAT format = info.GetCurrentDisplayMode();

            Trace.WriteLine("Current Display mode = " + format.Width.ToString() + " x " + format.Height.ToString() +
                            " BPP = " + format.Format);

            Trace.WriteLine("Driver = " + info.GetDriverName());

            Trace.WriteLine("Pixel Shader Support = " + info.IsPixelShaderSupported());

            Trace.WriteLine("    max PS Version = " + info.GetMaxPixelShaderVersion());
            Trace.WriteLine("Vertex Shader Support = " + info.IsVertexShaderSupported());
            Trace.WriteLine("    max VS Version = " + info.GetMaxVertexShaderVersion());

            Trace.WriteLine("Max Anisotropy = " + info.GetMaxAnisotropy());
            Trace.WriteLine("Bump Mapping Support = " + info.IsBumpmappingSupported().ToString());
            Trace.WriteLine("Reflection Support = " + info.IsReflectionSupported());
            Trace.WriteLine("Stencil Shadow Support = " + info.IsStencilShadowsSupported());
            Trace.WriteLine("Double Sided Stencil Support = " + info.IsDoubleSidedStencilSupported());

            //Core._CoreClient.Engine.ResizeDevice(); // what does this do?


            // video adapters
            _adapterCount = info.GetAdapterCount();
            Trace.WriteLine("Adapter Cout = " + _adapterCount);

            _adapters = new string[_adapterCount];
            _displayModes = new string[_adapterCount][];

            for (int i = 0; i < _adapterCount; i++)
            {
                info.SetCurrentAdapter(i);
                _adapters[i] = info.GetAdapterName();

                Trace.WriteLine("Adapter ID  = " + info.GetAdapterID() + " Adapter Name = " + info.GetAdapterName());
                Trace.WriteLine("Adapter " + i + " Fullscreen Display Modes = " + info.GetDisplayModeCount());

                // video adapter display mode enumeration
                _displayModes[i] = new string[info.GetDisplayModeCount()];

                for (int j = 0; j < info.GetDisplayModeCount(); j++)
                {
                    format = info.GetDisplayMode(j);
                    string mode = format.Width + " x " + format.Height + " BPP = " + format.Format;
                    //Trace.WriteLine("     Mode " + j + " = " + mode);
                    _displayModes[i][j] = mode;
                }
            }
        }
    }
}