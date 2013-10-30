using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MaryKay.Configuration;
using Quartet.Entities.Questionnaire;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class QuestionnaireController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        IAppSettings _appSettings;

        public QuestionnaireController(IQuartetClientFactory clientFactory, IAppSettings appSettings)
        {
            _clientFactory = clientFactory;
            _appSettings = appSettings;
        }

        [AcceptVerbs("GET")]
        public IEnumerable<dynamic> QuestionnaireAnswers(Guid custId)
        {
            Guid questionnaireItemId;
            if (!Guid.TryParse(_appSettings.GetValue("QuestionnaireID"), out questionnaireItemId))
                throw new ArgumentException("QuestionnaireID is missing in the appSettings and is required to load the questionnaire");

            Uri imageRootUrl;
            if (!(Uri.TryCreate(Request.RequestUri, _appSettings.GetValue("Questionnaire.ImageRootUrl"), out imageRootUrl)))
                throw new ArgumentException("Questionnaire.ImageRootUrl is missing or invalid in App.config and is required to load the questionnaire");

            Func<string, Uri> MakeImageUrl = path =>
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                Uri url = null;
                Uri.TryCreate(imageRootUrl, path, out url);
                return url;
            };

            var customerQueryClient = _clientFactory.GetCustomersQueryServiceClient();
            var answerResult = customerQueryClient.GetQuestionnaire(custId);
            if (answerResult == null || answerResult.QuestionnaireItems == null)
                return Enumerable.Empty<dynamic>();

            var answers = answerResult.QuestionnaireItems
                .Where(a => !string.IsNullOrWhiteSpace(a.Value))
                .Select(a => new { QuestionReferenceCode = a.Key, Value = a.Value, Values = a.Value.Split('|') })
                .ToDictionary(a => a.QuestionReferenceCode, StringComparer.InvariantCultureIgnoreCase);

            var globalQueryClient = _clientFactory.GetGlobalQueryServiceClient();
            var questionnaire = globalQueryClient.GetQuestionnaireItemById(questionnaireItemId);

            var result =
                from g in questionnaire.QuestionGroups
                where g.IsActive && g.Questions.Any(q => answers.ContainsKey(q.QuestionReferenceCode))
                select new
                {
                    Key = g.Id,
                    Text = g.Name,
                    Questions =
                        from q in g.Questions
                        where q.IsActive && answers.ContainsKey(q.QuestionReferenceCode)
                        select new
                        {
                            Key = q.QuestionReferenceCode,
                            Text = q.Name,
                            Type = q.QuestionType,
                            FreeTextAnswer =
                                answers[q.QuestionReferenceCode].Values.Where(f =>
                                    !q.QuestionAnswerOptions.Any(z => z.AnswerReferenceCode.Equals(f, StringComparison.InvariantCultureIgnoreCase)))
                                    .FirstOrDefault(),
                            Answers =
                                q.QuestionType != QuestionAnswerTypes.FreeText ?
                                from qao in q.QuestionAnswerOptions
                                where
                                    qao.IsActive &&
                                    (answers[q.QuestionReferenceCode].Values.Contains(qao.AnswerReferenceCode, StringComparer.InvariantCultureIgnoreCase) ||
                                    answers[q.QuestionReferenceCode].Value.Equals(qao.AnswerReferenceCode, StringComparison.InvariantCultureIgnoreCase))
                                select new
                                {
                                    Key = qao.AnswerReferenceCode,
                                    Text = qao.Name,
                                    ImagePath = MakeImageUrl(qao.ImagePath)
                                } : null
                        }
                };

            return result;
        }

        [AcceptVerbs("GET")]
        public dynamic Questionnaire()
        {
            var queryServiceClient = _clientFactory.GetGlobalQueryServiceClient();
            var questionnaireId    = _appSettings.GetValue("QuestionnaireID");
            var questionnaire      = queryServiceClient.GetQuestionnaireItemById(new Guid(questionnaireId));

            Uri imageRootUrl;
            if (!(Uri.TryCreate(Request.RequestUri, _appSettings.GetValue("Questionnaire.ImageRootUrl"), out imageRootUrl)))
                throw new ArgumentException("Questionnaire.ImageRootUrl is missing or invalid in App.config and is required to load the questionnaire");

            Func<string, Uri> MakeImageUrl = path =>
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                Uri url = null;
                Uri.TryCreate(imageRootUrl, path, out url);
                return url;
            };

            var result =
                from g in questionnaire.QuestionGroups
                where g.IsActive
                select new
                {
                    Key = g.Id,
                    Text = g.Name,
                    Questions =
                        from q in g.Questions
                        where q.IsActive
                        select new
                        {
                            Key = q.QuestionReferenceCode,
                            Text = q.Name,
                            Type = q.QuestionType,
                            Answer = (string)null,
                            Answers = new string[0],
                            Options =
                                from qao in q.QuestionAnswerOptions
                                where qao.IsActive
                                select new
                                {
                                    Key = qao.AnswerReferenceCode,
                                    Text = qao.Name,
                                    AllowFreeText = qao.AllowFreeText,
                                    imageUrl = MakeImageUrl(qao.ImagePath)
                                }
                        }
                };

            return result;
        }
    }
}