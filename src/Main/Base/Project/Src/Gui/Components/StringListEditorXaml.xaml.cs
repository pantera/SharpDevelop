﻿/*
 * Erstellt mit SharpDevelop.
 * Benutzer: Peter Forstmeier
 * Datum: 23.03.2012
 * Zeit: 19:44
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using ICSharpCode.Core.Presentation;

namespace ICSharpCode.SharpDevelop.Gui
{
	/// <summary>
	/// Interaction logic for StringListEditorXaml.xaml
	/// </summary>
	public partial class StringListEditorXaml : UserControl
	{
		bool browseForDirectory;
		public event EventHandler ListChanged;
		
		public StringListEditorXaml()
		{
			InitializeComponent();
			moveUpButton.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.ArrowUp") };
			moveDownButton.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.ArrowDown")};
			deleteButton.Content = new Image { Height = 16, Source = PresentationResourceService.GetBitmapSource("Icons.16x16.DeleteIcon")};
			DataContext = this;
		}
		
		public string TitleText {get;set;}
		
		public string ListCaption {get;set;}
		
		
		public bool BrowseForDirectory {
			get {
				return browseForDirectory;
			}
			set {
				browseForDirectory = value;
				if (browseForDirectory) {
					browseButton.Visibility = Visibility.Visible;
				} else {
					browseButton.Visibility = Visibility.Hidden;
				}
			}
		}
		
		
		public bool AutoAddAfterBrowse {get;set;}
		
		
		void AddButton_Click(object sender, RoutedEventArgs e)
		{
			editTextBox.Text = editTextBox.Text.Trim();
			if (editTextBox.Text.Length > 0) {
				int index = listBox.Items.IndexOf(editTextBox.Text);
				if (index < 0) {
					index = listBox.Items.Add(editTextBox.Text);
					OnListChanged(EventArgs.Empty);
				}
				listBox.SelectedIndex = index;
			}
		}
		
		#region Load/Save List
		
		public void LoadList(IEnumerable<string> list)
		{
			listBox.Items.Clear();
			foreach (string str in list) {
				listBox.Items.Add(str);
			}
			CheckEnableState();
		}
		
		
		public string[] GetList()
		{
			string[] list = new string[listBox.Items.Count];
			for (int i = 0; i < list.Length; i++) {
				list[i] = listBox.Items[i].ToString();
			}
			return list;
		}
		
		
		#endregion
		
		
		private void UpdateButton_Click(object sender, RoutedEventArgs e)
		{
			editTextBox.Text = editTextBox.Text.Trim();
			if (editTextBox.Text.Length > 0) {
				listBox.Items[listBox.SelectedIndex] = editTextBox.Text;
				OnListChanged(EventArgs.Empty);
			}
		}
		
		
		private void BrowseButtonClick (object sender, EventArgs e)
		{
			using (System.Windows.Forms.FolderBrowserDialog fdiag = FileService.CreateFolderBrowserDialog("${res:Dialog.ProjectOptions.SelectFolderTitle}"))
			{
				if (fdiag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string path = fdiag.SelectedPath;
					if (!path.EndsWith("\\") && !path.EndsWith("/"))
						path += "\\";
					editTextBox.Text = path;
					
					if (AutoAddAfterBrowse) {
						AddButton_Click(null, null);
					}
				}
			}
		}
		
		
		private void EditTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			addButton.IsEnabled = editTextBox.Text.Length > 0;
//			updateButton.IsEnabled = listBox.SelectedIndex >= 0 && editTextBox.Text.Length > 0;
			CheckEnableState();
		}
		
		
		protected virtual void OnListChanged(EventArgs e)
		{
			if (ListChanged != null) {
				ListChanged(this, e);
			}
			CheckEnableState();
		}

		
		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (listBox.SelectedIndex >= 0) {
				editTextBox.Text = listBox.Items[listBox.SelectedIndex].ToString();
			}
			CheckEnableState();
		}

		
		private void CheckEnableState()
		{
			moveUpButton.IsEnabled = listBox.SelectedIndex > -1 && listBox.Items.Count > 1;
			moveDownButton.IsEnabled = listBox.SelectedIndex > -1 && listBox.Items.Count > 1;
			removeButton.IsEnabled = listBox.SelectedIndex > -1;
			
			deleteButton.IsEnabled = listBox.SelectedIndex > -1;
			updateButton.IsEnabled = listBox.SelectedIndex > -1 && editTextBox.Text.Length > 0;
		}
		
		
		#region MoveUp-MoveDow-DeleteButton
		
		private void MoveUpButtonClick(object sender, RoutedEventArgs e)
		{
			int index = listBox.SelectedIndex;
			object tmp = listBox.Items[index];
			listBox.Items[index] = listBox.Items[index - 1];
			listBox.Items[index - 1] = tmp;
//			listBox.SelectedIndex = index - 1;
			OnListChanged(EventArgs.Empty);
		}
		
		private void MoveDownButtonClick(object sender, RoutedEventArgs e)
		{
			int index = listBox.SelectedIndex;
			object tmp = listBox.Items[index];
			listBox.Items[index] = listBox.Items[index + 1];
			listBox.Items[index + 1] = tmp;
//			listBox.SelectedIndex = index + 1;
			OnListChanged(EventArgs.Empty);
		}
		
		
		private void RemoveButtonClick(object sender, RoutedEventArgs e)
		{
			if (listBox.SelectedIndex >= 0) {
				listBox.Items.RemoveAt(listBox.SelectedIndex);
				OnListChanged(EventArgs.Empty);
			}
		}
		
		#endregion
		
		
	}
}