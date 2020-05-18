using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(shop.Startup))]
namespace shop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
