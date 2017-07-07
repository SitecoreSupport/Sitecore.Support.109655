using Sitecore.Collections;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Pipelines.Save;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;

namespace Sitecore.Support.Pipelines.Save
{
    public class Validators
    {
        public void Process(SaveArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            this.ProcessInternal(args);
        }

        protected void ProcessInternal(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (args.Result == "no")
                {
                    args.AbortPipeline();
                }
                args.IsPostBack = false;
            }
            else
            {
                string formValue = WebUtil.GetFormValue("scValidatorsKey");
                if (!string.IsNullOrEmpty(formValue))
                {
                    ValidatorOptions options = new ValidatorOptions(true);
                    ValidatorCollection validators = ValidatorManager.GetValidators(ValidatorsMode.ValidatorBar, formValue);
                    ValidatorManager.Validate(validators, options);
                    Pair<ValidatorResult, BaseValidator> pair1 = ValidatorManager.GetStrongestResult(validators, true, true);
                    ValidatorResult result = pair1.Part1;
                    BaseValidator failedValidator = pair1.Part2;
                    if ((failedValidator != null) && failedValidator.IsEvaluating)
                    {
                        SheerResponse.Alert("The fields in this item have not been validated.\n\nWait until validation has been completed and then save your changes.", new string[0]);
                        args.AbortPipeline();
                    }
                    else
                    {
                        if (result != ValidatorResult.CriticalError)
                        {
                            if (result != ValidatorResult.FatalError)
                            {
                                return;
                            }
                        }
                        else
                        {
                            string str2 = Translate.Text("Some of the fields in this item contain critical errors.\n\nAre you sure you want to save this item?");
                            if ((failedValidator != null) && MainUtil.GetBool(failedValidator.Parameters["showvalidationdetails"], false))
                            {
                                str2 = str2 + ValidatorManager.GetValidationErrorDetails(failedValidator);
                            }
                            SheerResponse.Confirm(str2);
                            args.WaitForPostBack();
                            return;
                        }
                        string text = Translate.Text("Some of the fields in this item contain fatal errors.\n\nYou must resolve these errors before you can save this item.");
                        if ((failedValidator != null) && MainUtil.GetBool(failedValidator.Parameters["showvalidationdetails"], false))
                        {
                            text = text + ValidatorManager.GetValidationErrorDetails(failedValidator);
                        }
                        SheerResponse.Alert(text, new string[0]);
                        args.AbortPipeline();
                    }
                }
            }
        }
    }
}