

namespace GeneratorService.Core.User.Exceptions {
    public class UserNotFoundException : Exception {
        public UserNotFoundException() : base("User has not been found") { }

    }


    public class UserAlreadyExistsException : Exception {
        public UserAlreadyExistsException() : base("User already exists") { }

    }


    public class UserInvalidCredentialsException : Exception {
        public UserInvalidCredentialsException() : base("Invalid email or password") { }

    }


    public class UserAuthInternalException : Exception {
        public UserAuthInternalException() : base("Internal server error in UserAuth") { }

    }
}

