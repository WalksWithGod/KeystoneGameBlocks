using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Keystone.Converters;

namespace Keystone.Controllers
{
    // http://www.codeproject.com/Articles/117611/Delegates-in-C-Attempt-to-look-inside-Part-4
    public class Interpreter
    {
        // this class is responsible for loading scripts.
		// scripts are maintained as scripts containing tokens
		// and the scripts are interpreted at run-time.
		// i.e. They are not converted into any byte code
        // =======================================================================
        // public Structures 
        // =======================================================================   
        private struct STACK_ENTRY
        {
            public string callName;
            public int lineNum;
            public int linePos;
        }

        public struct DEBUG_SYMBOL
        {
            public int lineNum;
            // these are both just the starting line number and position of this scriptlet in the file

            public int[] argOffsets;
            // the line position of the start of each argument. This alone with lineNum make up symbolic debug info
        }

        public struct SCRIPTLET
        {
            public string script; // the original line. 
            public DEBUG_SYMBOL symbol;
        }


        private const int MAX_STACK_SIZE = 16;
        private const int MAX_LINE_LENGTH = 512;
        private const int MAX_TOKENS_PER_SCRIPTLET = 32;
        private bool _abortcurrentscript = false;
        // for tracking the depth of our scriptlet interpreter traversal
        // we can use a stack to push/pop scripts that we are running onto
        // this will make it easier to unwind after any errors while executing a particular command
        // stack.Count <-- tells us our depth
        private Stack<STACK_ENTRY> _callStack = new Stack<STACK_ENTRY>();

        // comparing _currentCommandKey to that of the previous one stored in the stackEntry can tell 
        // us if this is a malformed infite looping script that needs to be aborted
        private bool _detectAndAbortInfiniteLoops = true;
        private int _loopCount;
        private string _CommandKeyPrevious;
        private string _CommandKeyCurrent;
        private bool _runningRoutine = false;
        Keystone.CmdConsole.Console mConsole;

        InputControllerBase mInputController;

        // our scripts collection hosts the user Alias scripts 
        private Dictionary<string, SCRIPTLET> _scripts = new Dictionary<string, SCRIPTLET>();
        private string _filename = "";

        public Interpreter(Keystone.CmdConsole.Console console, InputControllerBase controller)
        {
            if (console == null || controller == null) throw new ArgumentNullException();
            mConsole = console;
            mInputController = controller ;
        }

        private bool scriptUnload()
        {
            //_bScriptsLoaded = false 
            //TODO: umm... should we track the loaded=true/false? 
            _scripts.Clear();
            return true;
        }


        //it's NOT an error for a new subroutine to attempt to reference another routine that doesnt yet exist.
        // forward referencing is OK since we validate all our scripts AFTER they are all loaded as individual
        // scriplets
        //TODO: some things should not be bindable (like ESC or ALT+ENTER combo and im sure lots more)
        public bool scriptLoad(string sFile, bool bValidate, bool AppendToExistingScripts)
        {
            List<string> sTokens;
            int i = 0;
            List<int> symbols = new List<int>();
            StreamReader reader = null;

            if (!(AppendToExistingScripts)) _scripts.Clear();

            try
            {
                reader = File.OpenText(sFile);

                string s;
                while ((s = reader.ReadLine()) != null)
                {
                    i++;
                    if (s.Length > 0)
                    {
                        if (s.Length <= MAX_LINE_LENGTH)
                        {
                            sTokens = Tokenizer.TokenizeWithSymbols(s, symbols);
                            if (sTokens.Count < MAX_TOKENS_PER_SCRIPTLET)
                            {
                                // look for intrinsic commands
                                switch (sTokens[0].ToLower())
                                {
                                    case "rem":
                                        break;
                                    case "alias":
                                        Interpret(s, i);
                                        break;
                                    case "bind":
                                        Interpret(s, i);
                                        break;
                                    case "echo":
                                        Interpret(s, i);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                throw new ArgumentOutOfRangeException("Interpreter.scriptLoad() - Max tokens exceeded.");
                            }
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Interpreter.scriptLoad() - Max lines in script exceeded.");
                        }
                    }
                }
                _filename = sFile;
                reader.Close();
                if (bValidate)
                    Validate();

                return true;
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException("Interpreter.scriptLoad() - File not found. " + ex.Message);
                // TODO: here we could re-call this sub with a default
                // if (sFile <> DEFAULTKEYBOARD.CONFIG ) 
                //     return Me.scriptLoad(DEFAULTKEYBOARD.CONFIG, true);
                // else 
                //     throw new Exception("Could not load any keyboard configuration file.  Check that you have a keyboard.config file in your installed directory."); 
                // }
            }
            catch (Exception ex)
            {
                //TODO: here actually i think its best to throw an exception and let the caller "catch" it
               // throw new Exception("Error loading file.");
            }
            finally
            {
                if (reader != null) reader.Close();
                // TODO: if there was an error, dont we have to load defaults?
            }
            return false;
        }


        // scriptlet validation is done AFTER all scriptlets are loaded
        // we can either add a bEnabled flag to our scriptlet structure or we can remove invalid scriptlets from the collection
        // if the scriptlet has the wrong number of tokens given the command type, its invalid
        // if number of tokens > max allowed its invalid
        // those types maybe fail to even load into the collection.

        // once all scripts are loaded
        // if scriptlet contains an unkown token command, invalid (this might need to be recursive every time because of dependancies for previously validated scriptlets becoming invalid)  So in other words, to validate a script for correctness
        // we must first recurse its tokens and its tokens tokens, etc.

        // runtime validation
        // next runtime correctness and try to weed out malicious scripts
        // if running the scriptlet has itself for a token,it results in the same scriptlet being called again (its an infinite loop) and is invalid

        private void Validate()
        {
            // scripts cannot have names that collide with registered functions?
            // TODO: we may want to pass in the script collection so that we can also validate ones that are typed into the console
            // also, this way we can have seperate dictionaries and merge them... keep this in mind with our LoadScript since we have an Append function
            // which is probably not desireable since it then requires are validation to revalidate already "good" scriptlets.  Although
            // actually some "new" scripts might have dependancies on the existing scripts... so maybe i do in fact have to re-validate the whole thing
            foreach (SCRIPTLET item in _scripts.Values)
            {
                // check if its a keyword then we can determine how many arguments (if any) to parse
                // and then invoke it.  Remember, at the end all scripts wind up being fully invoked but
                // since a single script can contain several invokeable commands (or aliases to more scripts)
                // we keep looping til we've reached the end
                //Dim sArg1 As String
                //Dim sArg2 As String

                // TODO: validate should do argument count and type crap 
            }
        }

        public void registerAlias(string sName, SCRIPTLET script)
        {
            // if the script by this name already exists, we replace it
            if (_scripts.ContainsKey(sName))
                _scripts[sName] = script;
            else
                _scripts.Add(sName, script);
        }

        // TODO: Perhaps rather than refer to these as "scripts" we should refer
        // to these as commands because really that's what they are and the main
        // thing they allow is compound commands where there are multiple commands on same line
        // or commands that refer to other commands.
        //
        // called from the controllers directly to execute a script
        // for now, the controllers never have to send args, which is good
        // because ive evidentally not supported args here,but they are supported in Interpret()
        // - i think part of the reason args arent supported is "sRoutineName" might actually be
        // a line of script that needs to be interpreted.
        public void executeScript(string axisName)
        {
            if (mConsole.Functions.ContainsKey (axisName))
            {
                mConsole.Execute(axisName);
                mInputController.HandleAxisStateChange (axisName, null);
            }
            else if (_scripts.ContainsKey(axisName))
                Interpret(axisName, _scripts[axisName].symbol.lineNum);
        }

        // a lineNumber of -1 could mean it was a script typed into the console
        public bool Interpret(string script, int lineNumber)
        {
            try
            {
                string sScript;
                List<int> symbols = new List<int>();
                List<string> sTokens;
                STACK_ENTRY stackEntry = new STACK_ENTRY();
                object[] args;

                if (_callStack.Count >= MAX_STACK_SIZE)
                    _abortcurrentscript = true;
                else if (_callStack.Count > 0)
                    //this symbol offset needs to be added to the previous stackEntry offset.
                    stackEntry.linePos = _callStack.Peek().linePos;


                stackEntry.callName = script;
                stackEntry.lineNum = lineNumber;
                _callStack.Push(stackEntry);
                sScript = script;


                Trace.Assert(!(symbols == null));
                sTokens = Tokenizer.TokenizeWithSymbols(sScript, symbols);
                // <-- passed byRef so this line position can be updated.  Technicall if the stack is a class level variable, we could just push this onto the stack and peek it from the calling function
                if (sTokens.Count >= MAX_TOKENS_PER_SCRIPTLET)
                    _abortcurrentscript = true;
                else
                {
                    Trace.Assert(sTokens.Count > 0, "Interpreter.Interpret() -- Error: No tokens in script.");
                    stackEntry.linePos += symbols[0];

                    if (mConsole.AxisIsRegistered (sTokens[0]))
                    //if (_functionTable.ContainsKey(sTokens[0]))
                    {
                        args = new object[sTokens.Count]; // Not -1 here because we add debug symbol as last arg array element
                        // minus 1 since the first is the command name, but NOT minus 2 since we add debug symbol as last arg
                        if (sTokens.Count >= 2)
	                        Array.Copy (sTokens.ToArray(), 1, args, 0, sTokens.Count - 1);

                        DEBUG_SYMBOL symb;
                        symb.lineNum = stackEntry.lineNum;
                        symb.argOffsets = symbols.ToArray();
                        args[sTokens.Count - 1] = (object) symb;
                        mConsole.Execute(sTokens[0], args);
                        //_functionTable[sTokens[0]].Invoke(args);
                        mInputController.HandleAxisStateChange (sTokens[0], args);
                    }
                    else if (_scripts.ContainsKey(sTokens[0]))
                    {
                        // find out how many semicolon delimited subscripts and then execute each
                        List<string> subTokens = Tokenizer.TokenizeWithSymbols(_scripts[sTokens[0]].script, symbols);
                        if (subTokens.Count >= MAX_TOKENS_PER_SCRIPTLET)
                            _abortcurrentscript = true;
                        else
                        {
                            for (int i = 0; i < subTokens.Count; i++)
                                //recurse;
                                Interpret(subTokens[i], _scripts[sTokens[0]].symbol.lineNum);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Interpreter.Interpret() -- Command '" + sTokens[0] + "' not recognized " + 
                    	                "in file " + _filename + " at line " + stackEntry.lineNum + " position " + stackEntry.linePos);
                    }
                }
                _callStack.Pop();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Interpreter.Interpret() - " + ex.Message);
                return false;
            }
        }
    }
}

//http://www.counter-script.net/index.php?id=25


//==================================================================
////4 Bullet Burst Fire: Glock// by: submitted by: Flying Ladders 
//Description: This script will shoot 4 bullets with burst fire glock instead of 3. Be sure to turn off when using other guns. Gives burst fire glock accuracy of semi automatic if you start off glock with semi auto.

//alias +fastglock "+attack;+attack2;+attack2;+attack" 
//alias -fastglock "-attack;-attack2;-attack2;-attack" 
//alias fg "fgon" 
//alias fgon "bind mouse1 +fastglock; developer 1; echo Fast Glock on; developer 0; alias fg fgoff" 
//alias fgoff "bind mouse1 +attack; developer 1; echo Fast Glock off; developer 0; alias fg fgon" 

//bind "v" "fg" 

//==================================================================
////AK Burst of Fire// by: submitted by: Rodger That! 
//Description: This script will control how many shots you fire, (1 shot, 3 shots, 5 shots or full auto) works best with the AK, thus the name.

//alias "dev0" "developer 0"
//alias "dev1" "developer 1"
//// Adjust the amount of waits in the next alias until you find the one that works the best according to your system and connection.
//alias w3 "wait; wait; wait; wait; wait"
//alias "cycle" "cyclesemi"
//alias "cycleauto" "bind mouse1 +attack; dev1; echo <-------------------- AUTO FIRE -------------------->; alias cycle cyclesingle"
//alias "cyclesingle" "bind mouse1 single; dev1; echo <-------------------- SINGLE FIRE -------------------->; alias cycle cyclesemi"
//alias "cyclesemi" "bind mouse1 semi; dev1; echo <-------------------- SEMIAUTO FIRE -------------------->; alias cycle cycleburst"
//alias "cycleburst" "bind mouse1 burst; dev1; echo <-------------------- BURST FIRE -------------------->; alias cycle cycleauto"
//alias "single" "+attack; w3; -attack"
//alias "semi" "+attack; w3; -attack; w3; +attack; w3; -attack; w3; +attack; w3; -attack"
//alias "burst" "+attack; w3; -attack; w3; +at 

//==================================================================
////Burst/Duel/Single/Normal Cycle Script// by: submitted by: Ronin 
//Description: This script is an addon really to the Counter-Script Crew's Fire Script, this one allows you to cycle through the different modes of firing. 

//echo Burst Fire - PGUP
//echo Duel Fire - home
//echo Single fire - ins
//echo Normal - Del
//alias duelfire "bind mouse1 duel2; developer 1; echo Duel Fire Enabled; developer 0; bind end burstfire"
//alias duel2 "+attack; wt3; -attack; wt3; +attack; wt3; -attack; wt3"
//alias wt3 "wait; wait; wait"
//alias burstfire "bind mouse1 burst3; developer 1; echo Burst Fire Enabled; developer 0; bind end standard"
//alias burst3 "+attack; wt3; -attack; wt3; +attack; wt3; -attack; wt3; +attack; wt3; -attack; wt3;"
//alias normal "bind mouse1 +attack; bind end burstfire; developer 1; echo Normal Fire Enabled; developer 0; bind end singlefire"
//alias singlefire "bind mouse1 single1; developer 1; echo Single Fire Enabled; developer 0; bind end duelfire"
//alias single1 "+attack; wt3; -attack; wt3; echo Single Fire Enabled"

//alias fireselect "fireselect1"
//alias fireselect1 "normal; alias fireselect fireselect2"
//alias fireselect2 "singlefire; alias fireselect fireselect1"


//==================================================================
////Enhanced Weapon Selection Script// by: submitted by: hoLy 
//Description: Select weapons by pushing a button. With this Script u wont have to here the anoying beeps when u choose them.

//alias "main1" "use weapon_sg550;use weapon_mac10;use weapon_aug;use weapon_xm1014;use weapon_p90;use weapon_tmp;use weapon_mp5navy;use weapon_ump45"
//alias "main2" "use weapon_m4a1;use weapon_awp;use weapon_g3sg1;use weapon_sg552;use weapon_scout;use weapon_m3;use weapon_m249;use weapon_ak47"
//alias "main3" "use weapon_fiveseven;use weapon_usp; use weapon_glock18; use weapon_deagle; use weapon_p228; use weapon_elite"
//alias "pri" "main1;main2"
//alias "sec" "main3"
//alias "mes" "use weapon_knife"
//alias "fbgr" "use weapon_flashbang"
//alias "hegr" "use weapon_hegrenade"
//alias "smgr" "use weapon_smokegrenade"
//alias "c4" "use weapon_c4"

//bind a pri
//bind b sec
//bind c mes
//bind d fbgr
//bind e hegr
//bind f smgr
//bind g c4
//==================================================================