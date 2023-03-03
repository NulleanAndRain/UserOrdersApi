namespace Nullean.UserOrdersApi.WebApi.Models
{
    public static class Constants
    {
        public static class RoleNames
        {
            public const string User = "user";
            public const string Admin = "admin";
        }

        // test only
        public static class TestGuids
        {
            public static readonly Guid product1 = new Guid("b2d6f83a-2557-4ee9-85c8-86db15638e35");
            public static readonly Guid product2 = new Guid("84c80a5e-2706-4a24-b830-c654b61c2110");
            public static readonly Guid product3 = new Guid("8cb2cc63-dcd2-48c8-8b90-1fa18bbf44d2");
            public static readonly Guid product4 = new Guid("5438ad2b-5cff-4357-b726-e7125d20a26f");
            public static readonly Guid product5 = new Guid("02107147-d1c4-49dc-acef-c9cf05009631");
        }
    }
}
