using System.Configuration;
using System.Web.Optimization;

namespace myCustomers.Web
{
    // TODO: refactor me. I'm getting messy
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bool enableCDN = false;
            bool.TryParse(ConfigurationManager.AppSettings["EnableCDN"], out enableCDN);
            bundles.UseCdn = enableCDN;

            bundles.Add(new ScriptBundle("~/script/globalize", "//ajax.aspnetcdn.com/ajax/globalize/0.1.1/globalize.min.js").Include("~/scripts/globalize.js"));
            bundles.Add(new ScriptBundle("~/script/jquery", "//ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js").Include("~/scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/script/jqueryui", "//ajax.aspnetcdn.com/ajax/jquery.ui/1.10.1/jquery-ui.min.js").Include("~/scripts/jquery-ui-{version}.js"));
            //bundles.Add(new ScriptBundle("~/script/knockout", "//ajax.aspnetcdn.com/ajax/knockout/knockout-2.3.0.js").Include("~/scripts/knockout-{version}.js"));
            bundles.Add(new ScriptBundle("~/script/knockout").Include("~/scripts/knockout-{version}.js"));
            bundles.Add(new ScriptBundle("~/script/bootstrap").Include("~/scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/script/utils").Include
            (
                "~/scripts/moment.js",    
                "~/scripts/json2.js",
                "~/scripts/knockout.command.js",
                "~/scripts/knockout.activity.js",
                "~/scripts/knockout.mapping-latest.js",
                "~/scripts/knockout.validation.js",
                "~/scripts/knockout.validation.i18n.js",
                "~/scripts/knockout.extensions.js",
                "~/scripts/phoneformat.js",
                "~/scripts/toastr.js",
                "~/scripts/jstorage.js",
                "~/scripts/linq.js",
                "~/scripts/jquery.form.js",
                "~/scripts/jquery.fileDownload.js",
                "~/scripts/jquery.livequery.js",
                "~/scripts/jquery.html5-placeholder-shim.js",
                "~/scripts/jquery.ajaxPoll.js",
                "~/scripts/timepicker.js"
            ));

            bundles.Add(new ScriptBundle("~/script/app").IncludeDirectory("~/scripts/app", "*.js", true));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/script/modernizr").Include("~/scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/content/css").Include
            (
                "~/content/bootstrap.css",
                "~/content/font-awesome.css",
                "~/content/toastr.css",
                "~/content/site.css",
                "~/content/bootstrap-timepicker.css",
                "~/content/jquery.ui.progressbar.css",
                "~/content/jquery-ui.bootstrap.css"
                //"~/content/jquery-ui.ie.css"
            ));
        }
    }
}
