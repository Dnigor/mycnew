using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using MaryKay.Configuration;
using NLog;

namespace myCustomers.Web.Services
{
    // TODO: Make this a IExportTemplateProivder and inject into exporeters. Favor composition over inheritance.
    public abstract class ExportServiceBase
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        IAppSettings _appSettings;
        IEnvironmentConfig _environmentConfig;
        ISubsidiaryAccessor _subsidiaryAccessor;

        public ExportServiceBase(IAppSettings appSettings, IEnvironmentConfig environmentConfig, ISubsidiaryAccessor subsidiaryAccessor)
        {
            _appSettings       = appSettings;
            _environmentConfig = environmentConfig;
            _subsidiaryAccessor = subsidiaryAccessor;
        }

        protected Stream GetLocalizedTemplate(string fileName)
        {
            Stream designerFileStream = null;

            var templatePath = "~/ExcelTemplates/";
            var regionTemplatePath = "~/ExcelTemplates/" + _environmentConfig.GetRegion() + "/";
            var subsidiaryTemplatePath = "~/ExcelTemplates/" + _environmentConfig.GetRegion() + "/" + _subsidiaryAccessor.GetSubsidiaryCode() +"/";
            _environmentConfig.GetRegion();
            subsidiaryTemplatePath = VirtualPathUtility.ToAbsolute(subsidiaryTemplatePath);

            designerFileStream = GetTemplate(subsidiaryTemplatePath, fileName);
            if (designerFileStream == null)
            {
                regionTemplatePath = VirtualPathUtility.ToAbsolute(regionTemplatePath);
                designerFileStream = GetTemplate(regionTemplatePath, fileName);
                if (designerFileStream == null)
                {
                    templatePath = VirtualPathUtility.ToAbsolute(templatePath);
                    designerFileStream = GetTemplate(templatePath, fileName);
                }
            }



            return designerFileStream;
        }

        Stream GetTemplate(string folderPath, string fileName)
        {
            var filePath = VirtualPathUtility.Combine(folderPath, fileName);
            if (HostingEnvironment.VirtualPathProvider.FileExists(filePath))
            {
                var file = HostingEnvironment.VirtualPathProvider.GetFile(filePath);
                return file.Open();
            }
            _logger.Warn(string.Format("Could not find template {0}{1}", folderPath, fileName));
            return null;
        }
    }
}