using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;
using System.Text.RegularExpressions;

namespace BookingSystem.Android.Helpers
{
    public enum InputTypes
    {
        Text,
        Username,
        Email,
        Phone,
        Integer,
        Number
    }

    public class InputValidation
    {
        public bool Required { get; set; }

        public int? Min { get; set; }

        public int? Max { get; set; }

        public int? Compare { get; set; }
    }

    public class InputBinding
    {
        public View View { get; set; }

        public InputValidation Validation { get; set; }

        public InputTypes Type { get; set; }

        public int ViewId { get; set; }

        public string Name { get; set; }

        public Action<View> OnBind;

        public InputBinding() { }

        public InputBinding(string name, int viewId)
        {
            ViewId = viewId;
            Name = name;
        }

        public InputBinding(string name, int viewId, bool required, InputTypes type = InputTypes.Text, int? min = null, int? max = null, int? compare = null)
        {
            Name = name;
            ViewId = viewId;
            Type = type;
            Validation = new InputValidation()
            {
                Required = required,
                Min = min,
                Max = max,
                Compare = compare
            };
        }

    }


    public class InputHandler
    {
        static readonly string[] PhonePrefixes = new string[]
        {
            "020","024","055","054",
            "027","023","050","057"
        };

        static Regex EmailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

        public static bool IsValidPhone(string value)
        {
            return PhonePrefixes.Any(x => value.StartsWith(x)) && value.Length == 10;
        }

        public static bool IsValidEmail(string email)
        {
            return EmailRegex.IsMatch(email);
        }

        class ErrorEntry
        {
            public string Tag { get; set; }

            public string CustomerError { get; set; }
        }

        private IDictionary<string, IEnumerable<ErrorEntry>> UserErrorMap = new Dictionary<string, IEnumerable<ErrorEntry>>();

        public InputBinding[] Bindings { get; private set; }

        /// <summary>
        /// Sets a custom error for the input
        /// </summary>
        /// <param name="name">The name of the input</param>
        /// <param name="tag">The tag for the given error</param>
        /// <param name="error">The custom error you wish to set</param>
        public void SetCustomError(string name, string tag, string error)
        {
            if (UserErrorMap.ContainsKey(name))
            {
                ((IList<ErrorEntry>)UserErrorMap[name]).Add(new ErrorEntry()
                {
                    Tag = tag,
                    CustomerError = error
                });
            }
            else
            {
                UserErrorMap[name] = new List<ErrorEntry>()
                {
                    new ErrorEntry()
                    {
                        CustomerError  = error,
                        Tag = tag
                    }
                };
            }
        }

        public void SetBindings(InputBinding[] bindings, ViewGroup container)
        {
            //  Set bindings
            Bindings = bindings;

            //
            foreach (var binding in Bindings)
            {
                var view = container.FindViewById(binding.ViewId);
                binding.OnBind?.Invoke(view);
                binding.View = view;
            }

        }

        protected string GetError(string name, string tag)
        {
            if (UserErrorMap.ContainsKey(name))
            {
                return UserErrorMap[name].FirstOrDefault(x => x.Tag == tag)?.CustomerError;
            }

            return null;
        }

        public IList<KeyValuePair<string, IEnumerable<string>>> ValidateInputs(bool updateInputError, params string[] skipNames)
        {
            List<KeyValuePair<string, IEnumerable<string>>> validationErrors = new List<KeyValuePair<string, IEnumerable<string>>>();

            string toastMessage = "";
            foreach (var binding in Bindings.Where(x => !skipNames.Contains(x.Name)))
            {
                List<string> frameErrors = new List<string>();

                var validation = binding.Validation;
                string value = GetInputValue(binding) ?? "";
                string name = binding.Name;

                if (validation != null)
                {
                    if (validation.Required && value.Length == 0)
                    {
                        frameErrors.Add(GetError(name, "required") ?? $"{name} is required!");
                    }

                    if (validation.Min != null)
                    {
                        if ((binding.Type == InputTypes.Number || binding.Type == InputTypes.Integer) && double.TryParse(value, out var _v) && _v < validation.Min)
                        {
                            frameErrors.Add(GetError(name, "min") ?? $"{name} cannot be less than {validation.Min}");
                        }
                        else if (value.Length < validation.Min)
                        {
                            frameErrors.Add(GetError(name, "min") ?? $"{name} must not be less than {validation.Min} characters!");
                        }
                    }

                    if (validation.Max != null)
                    {
                        if ((binding.Type == InputTypes.Number || binding.Type == InputTypes.Integer) && double.TryParse(value, out var _v) && _v > validation.Max)
                        {
                            frameErrors.Add(GetError(name, "min") ?? $"{name} cannot be greater than {validation.Max}");
                        }
                        else if (value.Length > validation.Max)
                        {
                            frameErrors.Add(GetError(name, "max") ?? $"{name} must not exceed {validation.Max} characters!");
                        }
                    }

                    if (validation.Compare != null)
                    {
                        string otherValue = GetInputValue(validation.Compare.Value);
                        if (otherValue != value)
                        {
                            frameErrors.Add(GetError(name, "compare") ?? "Values do not match");
                        }
                    }

                }

                switch (binding.Type)
                {
                    case InputTypes.Email:

                        if (!IsValidEmail(value))
                            frameErrors.Add(GetError(name, "email") ?? $"The value '{value}' isn't a valid email address");

                        break;
                    case InputTypes.Phone:

                        if (!IsValidPhone(value))
                            frameErrors.Add(GetError(name, "phone") ?? "Invalid phone number!");

                        break;
                    case InputTypes.Integer:
                        {
                            if (double.TryParse(value, out double v))
                            {
                                if ((v - (int)v) != 0)
                                    frameErrors.Add(GetError(name, "integer") ?? "Invalid value. Expected an integer");
                            }
                            else
                            {
                                frameErrors.Add(GetError(name, "integer") ?? "Invalid value. Expected an integer");
                            }
                        }
                        break;
                    case InputTypes.Number:
                        {
                            if (!double.TryParse(value, out double v))
                            {
                                frameErrors.Add(GetError(name, "number") ?? "Invalid value. Expected a number");
                            }
                        }
                        break;
                    case InputTypes.Username:

                        if (value.Length < 2 || value.Length > 16)
                        {
                            frameErrors.Add(GetError(name, "username") ?? "Username must not be less than 2 or greater than 16 letters");
                        }
                        else if (value.Any(x => !char.IsLetterOrDigit(x) || char.IsWhiteSpace(x)))
                        {
                            frameErrors.Add(GetError(name, "username") ?? "Username cannot contain any special characters including whitespaces");
                        }

                        break;
                }

                if (frameErrors.Count > 0)
                {
                    validationErrors.Add(new KeyValuePair<string, IEnumerable<string>>(name, frameErrors));
                }

                //  Clear input error first
                if (updateInputError)
                {
                    if (binding.View is EditText tbEdit)
                    {
                        if (frameErrors.Count > 0)
                        {
                            binding.View.RequestFocus();
                            toastMessage = frameErrors.First();
                        }
                    }
                    else if (binding.View is TextInputLayout inputEdit)
                    {
                        inputEdit.ErrorEnabled = false;
                        inputEdit.Error = null;

                        if (frameErrors.Count > 0)
                        {
                            inputEdit.ErrorEnabled = true;
                            inputEdit.Error = frameErrors.First();
                        }
                    }
                }
            }

            if(toastMessage?.Length > 0)
            {
                ToastHelpers.ToastMessage(CustomApplication.CurrentActivity, toastMessage);
            }

            return validationErrors;
        }

        public string GetInputValue(InputBinding binding)
        {
            if (binding.View is TextInputLayout input)
            {
                return input.EditText.TrimInput();
            }
            else if (binding.View is EditText textInput)
            {
                return textInput.TrimInput();
            }

            return null;
        }

        public string GetInputValue(int viewId)
        {
            var binding = Bindings.FirstOrDefault(x => x.ViewId == viewId);
            if (binding != null)
            {
                return GetInputValue(binding);
            }

            return null;
        }

        public IDictionary<string, string> GetInputs()
        {
            IDictionary<string, string> properties = new Dictionary<string, string>();
            foreach (var binding in Bindings)
                properties.Add(binding.Name, GetInputValue(binding));

            return properties;
        }



    }
}