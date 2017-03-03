using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BidARide.Startup))]
namespace BidARide
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
