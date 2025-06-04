using System;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Simulation
{
	// TODO: this should be the script for our sun_atmospheric.css (as opposed to star.css)
    // used to control sun (and other heavenly bodies) paths around the camera's position
    public class Satellite
    {
        //TODO: dont really need this struct.
        public struct AltAzAngles
        {
            public float altitude;
            public float azimuth;

            public AltAzAngles(float altitude, float azimuth)
            {
                this.altitude = altitude;
                this.azimuth = azimuth;
            }

            public AltAzAngles Invert()
            {
                return new AltAzAngles(-altitude, -azimuth);
            }
        }

        private const float DEGREES_PER_RADIAN = 57.2957795f;
        private const float PI = (float) Math.PI;
        private const float TWO_PI = (float) (Math.PI * 2d);
        private const float LATITUDE = 0.6981317f; // New York's latitude (40°N), in radians  // TODO: get latitude of Jersey
        // const double LATITUDE = 0.22f;        // Default latitude (20°N), in radians

        private bool _useRadians = false;
        private bool _computeAzimuthAndAltitude = true;
        private AltAzAngles _angles = new AltAzAngles();
        private float _altitudeRadians;
        private Vector3d _position = new Vector3d();
        private Vector3d _direction = new Vector3d();
        private GameTime _simTime;
        private float _orbitalRadius = 12500;
        protected float _frequency = 0; //update frequency of the simulation. 0 = every frame


        private float _julianDay;
                
        // TODO: simTime should instead come from SimulationAPI.GetTime()
        public Satellite(GameTime simTime, bool useRadians, bool computeAzimuthAndAltitude)
        {
            _useRadians = useRadians;
            _computeAzimuthAndAltitude = computeAzimuthAndAltitude;
            _simTime = simTime;
            
            
            // -- Julian date calculation
            // http://en.wikipedia.org/wiki/Julian_day
            // The Julian day or Julian day number (JDN) is the integer number of days that have elapsed since the initial
            // epoch defined as noon Universal Time (UT) Monday, January 1, 4713 BC in the proleptic Julian calendar.
            // That noon-to-noon day is counted as Julian day 0. Thus the multiples of 7 are Mondays. 
            // Negative values can also be used, although those predate all recorded history.

            // I discarded the year in the equation because a number so big made the double's run out
            // of decimals and caused precision errors. I don't think we care if we're in 1975.   
            DateTime _time = _simTime.Time;
            
            int a = (14 - _time.Month) /12;
            int y = 1975 + 4800 - a;
            int m = _time.Month + 12 * a - 3;
            _julianDay = _time.DayOfYear + (153 * m + 2) / 5 + y * 365 + y / 4 - y / 100 + y / 400 - 32045;
            _julianDay -= 2442414;
            _julianDay -= 1f / 24f;
        }

        public float JulianDay
        {
            get { return _julianDay + (float) _simTime.Time.TimeOfDay.TotalDays; }
        }

                
        public float AltitudeRadians
        {
            get { return _altitudeRadians; }
        }

        public AltAzAngles Angles
        {
            get { return _angles; }
        }

        public Vector3d Position
        {
            get { return _position; }
        }

        public Vector3d Direction
        {
            get { return _direction; }
        }

        public float OrbitalRadius
        {
            get { return _orbitalRadius; }
            set { _orbitalRadius = value; }
        }

        public void Update(float elapsed, Vector3d camPos)
        {
        	// compute new azimuth and altitude if we don't want sun's position to be fixed
            if (_computeAzimuthAndAltitude)
                _angles = CalculateSunPosition(_julianDay, LATITUDE);
            
            // TODO: i think our own math MoveAroundPoint works fine
            //camPos.y = 0;
            //_position = Keystone.Utilities.MathHelper.MoveAroundPoint (camPos, _orbitalRadius, Angles.azimuth, -Angles.altitude);
            _position = Helpers.TVTypeConverter.FromTVVector(
                CoreClient._CoreClient.Maths.MoveAroundPoint(
            		new TV_3DVECTOR((float)camPos.x, 0, (float)camPos.z), 
            		_orbitalRadius,
                    Angles.azimuth,
                    -Angles.altitude));
            
            // TODO: shouldn't this also use a camPos where y = 0?
            _direction = Vector3d.Normalize(_position - camPos);
        }

        /// <summary>
        /// A simplified algorithm for computing planetary positions.
        /// How to compute planetary positions, By Paul Schlyter, Stockholm, Sweden
        /// email: pausch@stjarnhimlen.se or WWW:http://stjarnhimlen.se/english.html 
        /// swedish - http://stjarnhimlen.se/
        /// </summary>
        /// <param name="julianDate"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        private AltAzAngles CalculateSunPosition(float julianDate, float latitude)
        {
            AltAzAngles angles;
            float gamma = 4.93073839645544f;

            // compute anomaly of elipse
            float meanAnomaly = 6.2398418f + 0.0172019696455f*julianDate;
            float eccAnomaly = 2f * (float) Math.Atan(1.016862146d * Math.Tan(meanAnomaly / 2d));
            eccAnomaly = meanAnomaly + 0.016720f*(float) Math.Sin(eccAnomaly);
            float trueAnomaly = 2f * (float) Math.Atan(1.016862146d * Math.Tan(eccAnomaly / 2d));
            float lambda = gamma + trueAnomaly;

            // right accension and declination from the anomaly
            float dec = (float) Math.Asin(Math.Sin(lambda) * 0.39778375791855d);
            float ra = (float) Math.Atan(Math.Tan(lambda) * 0.917479d);
            if (Math.Cos(lambda) < 0d) ra += (float) Math.PI;

            // latitude and longitude from
            float gha = 1.7457f + 6.300388098526f * julianDate;
            float latSun = dec;
            float lonSun = ra - gha;

            // To prevent over-calculation
            float cosLonSun = (float) Math.Cos(lonSun);
            float sinLonSun = (float) Math.Sin(lonSun);
            float cosLatSun = (float) Math.Cos(latSun);
            float sinLatSun = (float) Math.Sin(latSun);
            float sinLat = (float) Math.Sin(latitude);
            float cosLat = (float) Math.Cos(latitude);

            // compute altitude and azimuth
            angles.altitude = (float) Math.Asin(sinLat * sinLatSun + cosLat * cosLatSun * cosLonSun);
            float west = cosLatSun * sinLonSun;
            float south = -cosLat * sinLatSun + sinLat * cosLatSun * cosLonSun;
            angles.azimuth = (float) Math.Atan(west / south);

            if (south >= 0f) 
            	angles.azimuth = PI - angles.azimuth;
            else angles.azimuth = -angles.azimuth;
            if (angles.azimuth < 0f) 
            	angles.azimuth += TWO_PI;

            _altitudeRadians = angles.altitude;
            if (!_useRadians)
            {
                angles.azimuth *= DEGREES_PER_RADIAN;
                angles.altitude *= DEGREES_PER_RADIAN;
            }
            return angles;
        }
    }
}