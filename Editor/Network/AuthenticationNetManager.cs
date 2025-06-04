using System;
using System.Collections.Generic;

namespace KeyEdit.Network
{
    class AuthenticationNetManager // INetManager
    {

        Authentication.Algorithm mHashAlgorithm = Authentication.Algorithm.SHA256 ;
        Authentication.AuthenticatedLogin mAuthenticatedLogin;
        int mMaxTries;
        int mCurrentAttempt;

        Authentication.Algorithm AuthenticationHashAlgo
        {
            get { return mHashAlgorithm; }
            set { mHashAlgorithm = value; }
        }

        Authentication.AuthenticatedLogin AuthenticatedLogin
        {
            get { return mAuthenticatedLogin; }
            set { mAuthenticatedLogin = value; }
        }
        int MaxAuthenticationTries { get { return mMaxTries; } }
        int CurrentAuthenticationAttempt { get { return mCurrentAttempt; } }





    }
}
