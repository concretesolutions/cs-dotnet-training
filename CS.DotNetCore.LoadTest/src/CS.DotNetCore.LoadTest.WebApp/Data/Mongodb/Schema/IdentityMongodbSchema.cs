namespace CS.DotNetCore.LoadTest.WebApp.Data.Mongodb.Schema
{
    internal static class IdentityMongodbSchema
    {
        internal const string Collection = "Identity";
        internal const string IdentityNameProp = "identityName";
        internal const string IdentityNameIndex = "PK_IdentityName";
        internal const string SaltProp = "salt";
        internal const string PasswordProp = "password";
    }
}
