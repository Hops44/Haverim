namespace Haverim.Controllers.Helpers
{
    public static class GlobalVariables
    {
        public static string DefaultProfilePic = "http://laoblogger.com/images/default-profile-picture-5.jpg";
        public static string JWTSecretKey = "{secret}";
        public static string ImageApiBaseUrl = "http://localhost:59983";
        public static string ImageApiGetUrl = ImageApiBaseUrl + "/api/Image?name=";
    }
}
