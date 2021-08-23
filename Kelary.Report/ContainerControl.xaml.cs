/* 
 * Container for embedded document user controls.
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kelary.Report
{
	/// <summary>
	/// Interaction logic for ContainerControl.xaml
	/// </summary>
	public partial class ContainerControl : UserControl
	{
		public ContainerControl()
		{
			InitializeComponent();
			// Set style with resize adorner (disable adorner in designtime)
			if (!DesignerProperties.GetIsInDesignMode(this))
				Style = Resources["DesignerItemStyle"] as Style;

		}

		/// <summary>
		/// Gain focus when clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Surface_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (!IsFocused)
			{
				Control control = this;
				if (control != null)
				{
					Control scope = FocusManager.GetFocusScope(control) as Control;
					if (scope != null)
						FocusManager.SetFocusedElement(scope, control);
				}

				e.Handled = true;
			}
		}
	}
}
