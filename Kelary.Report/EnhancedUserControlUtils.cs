using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace Kelary.Report
{
    public static class EnhancedUserControlUtils
    {
        private static readonly Regex blockEmptyUIContainerRegex = new Regex(@"\<BlockUIContainer[^\>]*\>[^\<]*</BlockUIContainer\>", RegexOptions.Compiled);
        private static readonly Regex inlineEmptyUIContainerRegex = new Regex(@"\<InlineUIContainer[^\>]*\>[^\<]*</InlineUIContainer\>", RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentViewModel"></param>
        public static void Save(IEnhancedUserControl contentViewModel)
        {
            var name = contentViewModel.Name;
            ICloneable dataToPersist = contentViewModel.GetState();
            CompoundDocument CompoundDocument = CompoundDocumentFactory.Instance.CompoundDocument;
            CompoundDocument.PersitedTypes[name] = contentViewModel.GetType();
            CompoundDocument.PersitedValues[name] = dataToPersist;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CompoundDocument"></param>
        /// <returns></returns>
        public static ICloneable Load(string name, CompoundDocument CompoundDocument)
        {
            return CompoundDocument.PersitedValues[name];
        }

        // Get control names of all UserControls in the specified document region.
        public static List<string> GetUserControlNames(TextPointer start, TextPointer end)
        {
            List<string> userControlNames = new List<string>();

            TextPointer current = start;

            // Scan through the document fragment...
            while (current.CompareTo(end) < 0)
            {
                // If we encounter something that could potentially
                // contain a UserControl...
                if (current.Parent is BlockUIContainer || current.Parent is InlineUIContainer)
                {

                    // Get the BlockUIContainer or InlilneUIContainer's  full XAML markup.
                    string containerMarkup = XamlWriter.Save(current.Parent);

                    XmlDocument xmlDocument = new XmlDocument();
                    //TODO This is bottleneck
                    xmlDocument.LoadXml(containerMarkup);

                    // Extract the Name attribute from the XAML markup.
                    XmlAttribute nameAttribute = xmlDocument.DocumentElement.FirstChild.Attributes["Name"];

                    string name = null;

                    if (nameAttribute != null && !string.IsNullOrEmpty(nameAttribute.Value))
                    {
                        name = nameAttribute.Value;
                    }
                    else
                    {
                        //Debug.Assert(false);
                    }

                    // Store the UserControl's name in the List, avoiding duplicates.
                    if (!userControlNames.Contains(name))
                    {
                        userControlNames.Add(name);
                    }
                }

                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }

            return userControlNames;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="CompoundDocument"></param>
        /// <returns></returns>
        private static Type GetType(string name, CompoundDocument CompoundDocument)
        {
            return CompoundDocument.PersitedTypes[name];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xaml"></param>
        /// <returns></returns>
        private static string ExtractName(string xaml)
        {
            string name;

            const string pattern = "Name=";

            int startPos = xaml.IndexOf(pattern);

            if (startPos < 0)
            {
                throw new Exception(string.Format(@"Cannot extact Name attribute from ""{0}""", xaml));
            }

            name = xaml.Substring(startPos + pattern.Length + 1);

            int endPos = name.IndexOf('"');

            if (endPos < 0)
            {
                throw new Exception(string.Format(@"Cannot extact Name attribute from ""{0}""", xaml));
            }

            name = name.Substring(0, endPos);

            return name;
        }

        /// <summary>
        /// Replace controls with printing markup.
        /// </summary>
        /// <param name="xaml"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        /// <returns></returns>
        public static string ReplaceControlsWithMarkup(string xaml, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            string[] controlTypeNames = Factory.ControlTypeNames;

            foreach (string controlTypeName in controlTypeNames)
            {
                // Use a non-greedy search so that we don't grab multiple elements.
                // *? = Repeat any number of times, but as few as possible.
                string regexPattern = string.Format(@"\<{0} .*?\/>.*?</{0}\>", XamlTypeName<ContainerControl>());
                Regex regex = new Regex(regexPattern);

                MatchCollection matches = regex.Matches(xaml);

                foreach (Match match in matches)
                {
                    string name = ExtractName(match.Value);

                    var dataToPersist = EnhancedUserControlUtils.Load(name, CompoundDocument);

                    // Instantiate the control.
                    Type type = EnhancedUserControlUtils.GetType(name, CompoundDocument);

                    var view = Factory.CreateControl(type) as FrameworkElement;
                    var viewModel = view.DataContext as IEnhancedUserControl;

                    // Load it.
                    viewModel.Load(dataToPersist);

                    // Extract control's print markukp.
                    string markup = viewModel.GetPrintMarkup();

                    // Replace control with the print markup.
                    xaml = regex.Replace(xaml, markup, 1);
                }
            }

            return xaml;
        }

        /// <summary>
        /// Convert UserControls into PlaceholderControls in the xaml markup.
        /// </summary>
        /// <param name="xaml"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static string ReplaceControlsWithPlaceholders(string xaml, TextPointer start, TextPointer end, EnhancedUserControlFactory Factory)
        {
            // Get a list of UserControl Name attribute values in the 
            // order in which they appear in the document.
            //List<string> userControlNames = GetUserControlNames(start, end);
            string placeHolderControlTypeName = XamlTypeName<PlaceholderControl>();
            string assembly = typeof(PlaceholderControl).Assembly.GetName().Name;
            string contentControlTypeName = XamlTypeName<ContainerControl>();

            // Replace ContentControls with PlaceHolder control haveing
            // the same name as ContentControl.
            string enhancedUserControl = string.Format(@"<{0} .*?Name=""(?<name>[^""]*)"".*?</{0}>", contentControlTypeName);
            string placeHolderControl = string.Format(@"<{0} Name=""${{name}}"" Visibility=""Hidden""/>", placeHolderControlTypeName);
            Regex enhancedUserControlRegex = new Regex(enhancedUserControl); //, RegexOptions.Compiled
            return enhancedUserControlRegex.Replace(xaml, placeHolderControl);
        }

        /// <summary>
        /// Get short form of namespace for xaml
        /// </summary>
        /// <param name="Namespace"></param>
        /// <returns></returns>
        public static string ShortNamespace(string Namespace)
        {
            return string.Concat(Namespace.Where(x => Char.IsUpper(x))).ToLower();
        }

        public static string XamlTypeName<T>()
        {
            Type type = typeof(T);
            return ShortNamespace(type.Namespace) + ":" + type.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xaml"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string ReplaceUIContainersWithPlaceholders(string xaml, TextPointer start, TextPointer end)
        {
            string modifiedMarkup = xaml;

            List<string> userControlNames = GetUserControlNames(start, end);

            string clrns = typeof(PlaceholderControl).Namespace;
            string xmlns = ShortNamespace(clrns);
            string placeHolderControlTypeName = XamlTypeName<PlaceholderControl>();
            string assembly = typeof(PlaceholderControl).Assembly.GetName().Name;

            foreach (string name in userControlNames)
            {
                string placeHolderControl = string.Format(@"<{0} Name=""{1}"" Visibility=""Hidden""/>", placeHolderControlTypeName, name);
                string blockUIContainer = string.Format(@"<BlockUIContainer xmlns:{0}=""clr-namespace:{1};assembly={2}"">{3}</BlockUIContainer>", xmlns, clrns, assembly, placeHolderControl);
                string inlineUIContainer = string.Format(@"<InlineUIContainer xmlns:{0}=""clr-namespace:{1};assembly={2}"">{3}</InlineUIContainer>", xmlns, clrns, assembly, placeHolderControl);
                modifiedMarkup = blockEmptyUIContainerRegex.Replace(modifiedMarkup, blockUIContainer, 1);
                modifiedMarkup = inlineEmptyUIContainerRegex.Replace(modifiedMarkup, inlineUIContainer, 1);
            }

            return modifiedMarkup;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        public static void ReplacePlaceholdersWithRealControls(FlowDocument document, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            ReplacePlaceholdersWithRealControls(document, null, Factory, CompoundDocument);
        }

        /// <summary>
        /// Convert all PlaceholderControls in the document with the UserControls that they represent.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="richTextBox"></param>
        /// <param name="Factory"></param>
        /// <param name="CompoundDocument"></param>
        public static void ReplacePlaceholdersWithRealControls(FlowDocument document, RichTextBox richTextBox, EnhancedUserControlFactory Factory, CompoundDocument CompoundDocument)
        {
            int replacements = 0;

            TextPointer current = document.ContentStart;

            // Scan through the entire document...
            while (current.CompareTo(document.ContentEnd) < 0)
            {
                // UserControls will be nested in BlockUIContainer or InlineUIContainer XAML elements.
                BlockUIContainer blockUIContainer = current.Parent as BlockUIContainer;
                InlineUIContainer inlineUIContainer = current.Parent as InlineUIContainer;

                // If we found a BlockUIContainer or InlineUIContainer...
                if (blockUIContainer != null || inlineUIContainer != null)
                {
                    PlaceholderControl placeHolderControl;

                    if (blockUIContainer != null)
                    {
                        placeHolderControl = blockUIContainer.Child as PlaceholderControl;
                    }
                    else
                    {
                        placeHolderControl = inlineUIContainer.Child as PlaceholderControl;
                    }

                    // If we found a PlaceholderControl...
                    if (placeHolderControl != null)
                    {
                        // Determine the type of the UserControl that the 
                        // PlaceholderControl represents.
                        Type controlType = EnhancedUserControlUtils.GetType(placeHolderControl.Name, CompoundDocument);

                        // Create a new UserControl of the proper type.
                        var newElement = Factory.CreateControl(controlType);
                        IEnhancedUserControl viewModel = (newElement as Control).DataContext as IEnhancedUserControl;

                        // Retrieve the UserControl's state data.
                        ICloneable dataToPersist = (ICloneable)EnhancedUserControlUtils.Load(placeHolderControl.Name, CompoundDocument);

                        // Clone the state data so that the new UserControl won't overwrite
                        // another UserControl's data.
                        ICloneable newObject = (ICloneable)dataToPersist.Clone();

                        // Save the new UserControl's type and state.
                        CompoundDocument.PersitedTypes[viewModel.Name] = viewModel.GetType();
                        CompoundDocument.PersitedValues[viewModel.Name] = newObject;

                        // Load the new UserControl with its state data.
                        viewModel.Load(newObject);

                        // Nest the new UserControl inside the BlockUIContainer
                        // or InlineUIContainer.

                        if (blockUIContainer != null)
                        {
                            blockUIContainer.Child = newElement;
                        }
                        else if (inlineUIContainer != null)
                        {
                            inlineUIContainer.Child = newElement;
                        }
                        replacements++;
                    }
                }

                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }

            // Force RichTextBox to redraw and display the changed content.
            /*
			if (replacements > 0 & richTextBox != null)
			{
				TextPointer caretPosition = richTextBox.CaretPosition;

				FlowDocument save = richTextBox.Document;
				richTextBox.Document = new FlowDocument();
				richTextBox.Document = save;

				richTextBox.CaretPosition = caretPosition;
			}
			*/
        }
    }
}
