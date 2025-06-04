using System;
using System.Threading;


namespace keymath.ParseHelper
{
    /// <summary>
    /// To ensure we read/write persisted Types properly, always use "en-US" culture for IO.
    /// - eg. forces parse of "." for decimal instead of "," for German 
    ///   eg. forces use of en delimiters
    /// </summary>
    public class English
    {
        private static System.Globalization.CultureInfo mEnglishCulture;
        private static char[] mEnglishDelimiterChars;
        private static string mEnglishDelimiter;
        private static string mNestedDelimiter;
        private static string mXMLAttributeDelimiter;
        private static char[] mXMLAttributeDelimiterChars;
        private static string mXMLAttributeNestedDelimiter;
        private static char[] mXMLAttributeNestedDelimiterChars;
        private static bool mIsInitialized;

        public static System.Globalization.CultureInfo Culture
        {
            get
            {
                if (mIsInitialized == false)
                    Initalize();

                return mEnglishCulture;
            }
        }

        public static string Delimiter
        {
            get 
            {
                if (mIsInitialized == false)
                    Initalize();

                return mEnglishDelimiter; 
            }
        }
        
        public static string NestedDelimiter
        {
            get 
            {
                if (mIsInitialized == false)
                    Initalize();

                return mNestedDelimiter; 
            }
        }
                

        public static char[] DelimiterChars
        {
            get
            {
                if (mIsInitialized == false)
                    Initalize();

                return mEnglishDelimiterChars;
            }
        }
        

        public static string XMLAttributeDelimiter 
        {
            get 
            {
                if (mIsInitialized == false)
                    Initalize();

                return mXMLAttributeDelimiter; 
            }
        }

        public static string XMLAttributeNestedDelimiter
        {
            get
            {
                if (mIsInitialized == false)
                    Initalize();

                return mXMLAttributeNestedDelimiter;
            }
        }
        
        public static char[] XMLAttributeDelimiterChars
        {
            get
            {
                if (mIsInitialized == false)
                    Initalize();

                return mXMLAttributeDelimiterChars;
            }
        }

        public static char[] XMLAttributeNestedDelimiterChars
        {
            get
            {
                if (mIsInitialized == false)
                    Initalize();

                return mXMLAttributeNestedDelimiterChars;
            }
        }
        
        private static void Initalize()
        {
            if (mIsInitialized) return;

            
            //mCurrentCulture = new System.Globalization.CultureInfo("en-US");
            mEnglishCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

            // TODO: only our IO threads need to use this culture right?  and any parsing of xml and type data should use it
            // if somehow done from gui thread
            Thread.CurrentThread.CurrentCulture = mEnglishCulture;
            //Thread.CurrentThread.CurrentUICulture = mCurrentCulture; // do we want to change UI too? surely no! only for IO


            // english delimiter is comma
            mEnglishDelimiter = mEnglishCulture.TextInfo.ListSeparator;
            mEnglishDelimiterChars = mEnglishDelimiter.ToCharArray();
            System.Diagnostics.Debug.WriteLine("English text delimiter is '"  + mEnglishDelimiter + "'");
			
            mNestedDelimiter = ";";
            
            // TODO: i really f'd up my IO by allowing two chars to be used ' ' and ','
            mXMLAttributeDelimiter = ",";
            mXMLAttributeNestedDelimiter = ";";
            	
            // mXMLAttributeDelimiterChars = mXMLAttributeDelimiter.ToCharArray(); 
            // TODO: until i resave all my prefabs, i need to keep use of both space and comma since i mixed their use
            // apparently! ugh indeed. but all subsequent writes will use just the string comma so ill just be able to 
            // remove the space delim
            mXMLAttributeDelimiterChars = new char[] {' ',','};
            mXMLAttributeNestedDelimiterChars = new char[]{';'};
            mIsInitialized = true;
        }
    }
}
