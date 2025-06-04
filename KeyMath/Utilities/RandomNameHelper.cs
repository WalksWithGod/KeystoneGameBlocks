using System;

namespace Keystone.Utilities
{
	/// <summary>
	/// Description of RandomNameHelper.
	/// </summary>
	public class RandomNameHelper
	{
		private static Random mRand = new Random();
		            
		//public static RandomNameHelper (int seed, string configurationFile)
		//{
		//	mRand = new Random(seed);
			
			// load xml configuration information if available
			// the configuration file stores the list of countries
			// and the percentages of each to use.  If the total % reaches 100%
			// prior to end of file, then the rest of the file is ignored.  If 
			// the % is under 100% and the next entry would put it over 100% then
			// again, the rest of the list is ignored and the previous entry will
			// take up the slack.
//			Settings.Initialization ini = Settings.Initialization.Load (configurationFile );
//			if (ini == null) return;
			
			// load name resource files for each member of the alliance
//			string[][] firstNames;
			
			
		
			// crew generation
			// - determine how many crew for each job function we need
			
			// generate human character names for starship crew
			
			
			// Sol Alliance

		
			// generate alien species names
			
//		}

		// https://forums.tigsource.com/index.php?topic=22176.800
		public static string GenerateCivilizationName()
		{
			// __ Order
			// __ Hive / Swarm
			// __ Sovereignty
			// __ Collective
			// __ Federation
			// __ Union __ (eg __ Union of Planets, Union of Worlds, Union of Aligned Worlds)
			// __ Hegemony
			// __ Alliance
			// __ Aggregate
			// __ Republic
			// __ Confederacy
			// __ Assembly
			// __ Brotherhood
			// __ Dominion
			// __ Priestdom
			// Holy Land of __
			// __ Imperium
			// ___ Ecclesiarchy
			// __ Empire
			// Brood of __
			// Children of __
			// Deciples of __
			// Folk of __
			// Descendands of __
			// Heirs of __
			// Hagiocracy of __
			// Horsemen of __
			// Kin of __
			// Kindred of __
			// Kingdom of __
			// Line of __
			// Nation of __
			// Nomads of __
			// People of __
			// Progeny of __
			// Riders of __
			// Roamers of __
			// Traders of __
			// Tribe of __
			// Wanderers of __
			return null;
		}
		
		public static string GenerateStarName ()
		{
			int length = mRand.Next (3, 12);
			return GenerateStarName (length);
		}
		
		public static string GenerateStarName(int len)
        {
            string[] consonants = { "b", "c", "ch", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "ph", "q", "r", "s", "sh", "zh", "t", "th","v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[mRand.Next(consonants.Length)];
            char c = Name[0];
            Name = Name.Replace (c, c.ToString().ToUpper().ToCharArray()[0]);
            
            Name += vowels[mRand.Next(vowels.Length)];
            int b = Name.Length; //how many times a new letter has been added. 
            while (b < len)
            {
                Name += consonants[mRand.Next(consonants.Length)];
                b++;
                Name += vowels[mRand.Next(vowels.Length)];
                b++;
            }
            
            //System.Diagnostics.Debug.WriteLine ("RandomNameHelper.GenerateStarName() - Name '" + Name + "' generated.");

            return Name;
        }
		
		private int mSystemCount = 0;
        private string GetSystemName()
        {
        	int timesThrough = mSystemCount / 26; // +1 gives us our length
        	int remainder = mSystemCount % 26;    // gives us our character
        	
        	char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        	
        	string result = "";
        	for (int i = 0; i < timesThrough + 1; i++)
        		result += alpha[remainder];
        	
//        		if (mSystemCount > 26)
//        			System.Diagnostics.Debug.WriteLine ("GenerateStellarSystem.GetSystemName() - '" + result.ToString() + "'");
        		
       		mSystemCount++;
        	return result;
        	
//            return Keystone.Resource.Repository.GetNewName(typeof (Star));
//
//            // //This function generates made up names for star systems
//
//            // //generate a random number between 4 and 10
//            int n = _random.Next(4, 10);
//            for (int j = 1; j <= n; j++)
//            {
//                // now generate a random letter from the alphabet
//                int k = _random.Next(65, 90);
//                result = result + (char) k;
//            }
//            return result;
        }
	}
}
