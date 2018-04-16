namespace Refundeo.Core.Helpers
{
    public abstract class SignInResult
    {
        public abstract SignInId Id { get; }
        public abstract string Desc { get; }

        public class WrongPassword : SignInResult
        {
            public override SignInId Id { get { return SignInId.WRONG_PASSWORD; } }
            public override string Desc { get { return "Wrong password"; } }
        }

        public class UserDoesNotExist : SignInResult
        {
            public override SignInId Id { get { return SignInId.USER_DOES_NOT_EXIST; } }
            public override string Desc { get { return "User does not exist"; } }
        }

        public class NoPassword : SignInResult
        {
            public override SignInId Id { get { return SignInId.NO_PASSWORD; } }
            public override string Desc { get { return "No password provided"; } }
        }

        public class NoUsername : SignInResult
        {
            public override SignInId Id { get { return SignInId.NO_USERNAME; } }
            public override string Desc { get { return "No username provided"; } }
        }

        public class Success : SignInResult
        {
            public override SignInId Id { get { return SignInId.SUCCESS; } }
            public override string Desc { get { return "Success"; } }
        }
    }
}