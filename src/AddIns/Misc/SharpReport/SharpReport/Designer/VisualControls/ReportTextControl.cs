/*
 * Created by SharpDevelop.
 * User: Fabio
 * Date: 09/10/2004
 * Time: 9.30
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.Windows.Forms;

using SharpReportCore;

namespace SharpReport.Designer{
	/// <summary>
	/// Description of ReportTextItem.
	/// </summary>
	internal class ReportTextControl : ReportControlBase{

		private StringTrimming stringTrimming;
		private ContentAlignment contentAlignment;
		TextDrawer textDrawer  = new TextDrawer();
		
		
		public ReportTextControl():base(){
			InitializeComponent();
			
			this.SetStyle(ControlStyles.DoubleBuffer |
			              ControlStyles.UserPaint |
			              ControlStyles.AllPaintingInWmPaint |
			              ControlStyles.ResizeRedraw,
			              true);
			this.UpdateStyles();
			this.Size = GlobalValues.PreferedSize;
		}
		
		
		
		
		public override string Text{
			get { return base.Text; }
			set { base.Text = value;}
		}
		
		
		public  StringTrimming StringTrimming {
			set {
				stringTrimming = value;
				this.Invalidate();
			}
		}
		
		
		public System.Drawing.ContentAlignment ContentAlignment {
			set {
				this.contentAlignment = value;
				this.Invalidate();
			}
		}
	
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs pea){
		
			base.OnPaint(pea);
			base.DrawEdges (pea);
			base.DrawDecorations(pea);

			string str;
			
			if (String.IsNullOrEmpty(this.Text)) {
				str = this.Name;
			} else {
				str = this.Text;
			}
			
			this.textDrawer.DrawString (pea.Graphics,this.Text,this.Font,
			                            new SolidBrush(this.ForeColor),(RectangleF)this.ClientRectangle,
			                            this.stringTrimming,this.contentAlignment);
		}
		
		
		#region Windows Forms Designer generated code
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent() {
			// 
			// ReportTextItem
			// 
			this.BackColor = System.Drawing.Color.White;
			this.Name = "ReportTextItem";
			this.Size = new System.Drawing.Size(120, 20);
		}
		#endregion
		
	}
}
