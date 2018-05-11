namespace Refundeo.Core.Helpers
{
    public abstract class SignInResult
    {
        public abstract SignInId Id { get; }
        public abstract string Desc { get; }

        public class WrongPassword : SignInResult
        {
            public override SignInId Id => SignInId.WrongPassword;
            public override string Desc => "Wrong password";
        }

        public class UserDoesNotExist : SignInResult
        {
            public override SignInId Id => SignInId.UserDoesNotExist;
            public override string Desc => "User does not exist";
        }

        public class NoPassword : SignInResult
        {
            public override SignInId Id => SignInId.NoPassword;
            public override string Desc => "No password provided";
        }

        public class NoUsername : SignInResult
        {
            public override SignInId Id => SignInId.NoUsername;
            public override string Desc => "No username provided";
        }

        public class Success : SignInResult
        {
            public override SignInId Id => SignInId.Success;
            public override string Desc => "Success";
        }
    }
}
