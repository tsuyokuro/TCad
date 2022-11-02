using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;

namespace TCad.ScriptEditor.Search
{
	public partial class SearchPanel : UserControl
	{
		TextArea textArea;
		SearchInputHandler handler;
		TextDocument currentDocument;
		SearchResultBackgroundRenderer renderer;
		TextBox searchTextBox;
		Popup dropdownPopup;
		SearchPanelAdorner adorner;

		#region DependencyProperties
		public static readonly DependencyProperty UseRegexProperty =
			DependencyProperty.Register("UseRegex", typeof(bool), typeof(SearchPanel),
										new FrameworkPropertyMetadata(false, SearchPatternChangedCallback));

		public bool UseRegex {
			get { return (bool)GetValue(UseRegexProperty); }
			set { SetValue(UseRegexProperty, value); }
		}

		public static readonly DependencyProperty MatchCaseProperty =
			DependencyProperty.Register("MatchCase", typeof(bool), typeof(SearchPanel),
										new FrameworkPropertyMetadata(false, SearchPatternChangedCallback));

		public bool MatchCase {
			get { return (bool)GetValue(MatchCaseProperty); }
			set { SetValue(MatchCaseProperty, value); }
		}

		public static readonly DependencyProperty WholeWordsProperty =
			DependencyProperty.Register("WholeWords", typeof(bool), typeof(SearchPanel),
										new FrameworkPropertyMetadata(false, SearchPatternChangedCallback));

		public bool WholeWords {
			get { return (bool)GetValue(WholeWordsProperty); }
			set { SetValue(WholeWordsProperty, value); }
		}

		public static readonly DependencyProperty SearchPatternProperty =
			DependencyProperty.Register("SearchPattern", typeof(string), typeof(SearchPanel),
										new FrameworkPropertyMetadata("", SearchPatternChangedCallback));

		public string SearchPattern {
			get {
                return (string)GetValue(SearchPatternProperty);
            }
			set {
                SetValue(SearchPatternProperty, value);
            }
		}

		public static readonly DependencyProperty MarkerBrushProperty =
			DependencyProperty.Register("MarkerBrush", typeof(Brush), typeof(SearchPanel),
										new FrameworkPropertyMetadata(Brushes.LightGreen, MarkerBrushChangedCallback));

		public Brush MarkerBrush {
			get { return (Brush)GetValue(MarkerBrushProperty); }
			set { SetValue(MarkerBrushProperty, value); }
		}

		private static void MarkerBrushChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SearchPanel panel) {
				panel.renderer.MarkerBrush = (Brush)e.NewValue;
			}
		}

		public static readonly DependencyProperty MarkerPenProperty =
			DependencyProperty.Register("MarkerPen", typeof(Pen), typeof(SearchPanel),
										new PropertyMetadata(null, MarkerPenChangedCallback));

		public Pen MarkerPen {
			get { return (Pen)GetValue(MarkerPenProperty); }
			set { SetValue(MarkerPenProperty, value); }
		}

		private static void MarkerPenChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SearchPanel panel) {
				panel.renderer.MarkerPen = (Pen)e.NewValue;
			}
		}

		public static readonly DependencyProperty MarkerCornerRadiusProperty =
			DependencyProperty.Register("MarkerCornerRadius", typeof(double), typeof(SearchPanel),
										new PropertyMetadata(3.0, MarkerCornerRadiusChangedCallback));

		public double MarkerCornerRadius {
			get { return (double)GetValue(MarkerCornerRadiusProperty); }
			set { SetValue(MarkerCornerRadiusProperty, value); }
		}

		private static void MarkerCornerRadiusChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SearchPanel panel) {
				panel.renderer.MarkerCornerRadius = (double)e.NewValue;
			}
		}

		public static readonly DependencyProperty LocalizationProperty =
			DependencyProperty.Register("Localization", typeof(Localization), typeof(SearchPanel),
										new FrameworkPropertyMetadata(new Localization()));

		public Localization Localization {
			get { return (Localization)GetValue(LocalizationProperty); }
			set { SetValue(LocalizationProperty, value); }
		}
		#endregion

		static SearchPanel()
		{
            
		}

		ISearchStrategy strategy;

		static void SearchPatternChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SearchPanel panel = d as SearchPanel;
			if (panel != null) {
				panel.ValidateSearchText();
				panel.UpdateSearch();
			}
		}

		void UpdateSearch()
		{
			// only reset as long as there are results
			// if no results are found, the "no matches found" message should not flicker.
			// if results are found by the next run, the message will be hidden inside DoSearch ...
			if (renderer.CurrentResults.Any())
				messageView.IsOpen = false;
			strategy = SearchStrategyFactory.Create(SearchPattern ?? "", !MatchCase, WholeWords, UseRegex ? SearchMode.RegEx : SearchMode.Normal);
			OnSearchOptionsChanged(new SearchOptionsChangedEventArgs(SearchPattern, MatchCase, UseRegex, WholeWords));
			DoSearch(true);
		}

		public SearchPanel()
		{
            InitializeComponent();
        }

        private void SearchTextChanged(object sender, TextChangedEventArgs args)
        {
            TextBox t = sender as TextBox;
            if (t == null)
            {
                return;
            }
            SearchPattern = t.Text;
        }

        public static SearchPanel Install(TextEditor editor)
		{
			if (editor == null)
				throw new ArgumentNullException("editor");
			return Install(editor.TextArea);
		}

		public static SearchPanel Install(TextArea textArea)
		{
			if (textArea == null)
				throw new ArgumentNullException("textArea");
			SearchPanel panel = new SearchPanel();
			panel.AttachInternal(textArea);
			panel.handler = new SearchInputHandler(textArea, panel);
			textArea.DefaultInputHandler.NestedInputHandlers.Add(panel.handler);
			return panel;
		}

		public void RegisterCommands(CommandBindingCollection commandBindings)
		{
			handler.RegisterGlobalCommands(commandBindings);
		}

		public void Uninstall()
		{
			Close();
			textArea.DocumentChanged -= textArea_DocumentChanged;
			if (currentDocument != null)
				currentDocument.TextChanged -= textArea_Document_TextChanged;
			textArea.DefaultInputHandler.NestedInputHandlers.Remove(handler);
		}

		void AttachInternal(TextArea textArea)
		{
			this.textArea = textArea;
			adorner = new SearchPanelAdorner(textArea, this);
			DataContext = this;

			renderer = new SearchResultBackgroundRenderer();
			currentDocument = textArea.Document;
			if (currentDocument != null)
				currentDocument.TextChanged += textArea_Document_TextChanged;
			textArea.DocumentChanged += textArea_DocumentChanged;
			KeyDown += SearchLayerKeyDown;

			this.CommandBindings.Add(new CommandBinding(SearchCommands.FindNext, (sender, e) => FindNext()));
			this.CommandBindings.Add(new CommandBinding(SearchCommands.FindPrevious, (sender, e) => FindPrevious()));
			this.CommandBindings.Add(new CommandBinding(SearchCommands.CloseSearchPanel, (sender, e) => Close()));
			IsClosed = true;
		}

		void textArea_DocumentChanged(object sender, EventArgs e)
		{
			if (currentDocument != null)
				currentDocument.TextChanged -= textArea_Document_TextChanged;
			currentDocument = textArea.Document;
			if (currentDocument != null) {
				currentDocument.TextChanged += textArea_Document_TextChanged;
				DoSearch(false);
			}
		}

		void textArea_Document_TextChanged(object sender, EventArgs e)
		{
			DoSearch(false);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			searchTextBox = Template.FindName("PART_searchTextBox", this) as TextBox;
			dropdownPopup = Template.FindName("PART_dropdownPopup", this) as Popup;
		}

		void ValidateSearchText()
		{
			if (searchTextBox == null)
				return;

			var be = searchTextBox.GetBindingExpression(TextBox.TextProperty);

			try {
				if (be != null)
					Validation.ClearInvalid(be);

				UpdateSearch();

			} catch (SearchPatternException ex) {
				var ve = new ValidationError(be.ParentBinding.ValidationRules[0], be, ex.Message, ex);
				Validation.MarkInvalid(be, ve);
			}
		}

		public void Reactivate()
		{
			if (searchTextBox == null)
				return;
			searchTextBox.Focus();
			searchTextBox.SelectAll();
		}

		public void FindNext()
		{
			SearchResult result = renderer.CurrentResults.FindFirstSegmentWithStartAfter(textArea.Caret.Offset + 1);
			if (result == null)
				result = renderer.CurrentResults.FirstSegment;
			if (result != null) {
				SelectResult(result);
			}
		}

		public void FindPrevious()
		{
			SearchResult result = renderer.CurrentResults.FindFirstSegmentWithStartAfter(textArea.Caret.Offset);
			if (result != null)
				result = renderer.CurrentResults.GetPreviousSegment(result);
			if (result == null)
				result = renderer.CurrentResults.LastSegment;
			if (result != null) {
				SelectResult(result);
			}
		}

		ToolTip messageView = new ToolTip { Placement = PlacementMode.Bottom, StaysOpen = true, Focusable = false };

		void DoSearch(bool changeSelection)
		{
			if (IsClosed)
				return;
			renderer.CurrentResults.Clear();

			if (!string.IsNullOrEmpty(SearchPattern)) {
				int offset = textArea.Caret.Offset;
				if (changeSelection) {
					textArea.ClearSelection();
				}
				// We cast from ISearchResult to SearchResult; this is safe because we always use the built-in strategy
				foreach (SearchResult result in strategy.FindAll(textArea.Document, 0, textArea.Document.TextLength)) {
					if (changeSelection && result.StartOffset >= offset) {
						SelectResult(result);
						changeSelection = false;
					}
					renderer.CurrentResults.Add(result);
				}
				if (!renderer.CurrentResults.Any()) {
					messageView.IsOpen = true;
					messageView.Content = Localization.NoMatchesFoundText;
					messageView.PlacementTarget = searchTextBox;
				} else
					messageView.IsOpen = false;
			}
			textArea.TextView.InvalidateLayer(KnownLayer.Selection);
		}

		void SelectResult(SearchResult result)
		{
			textArea.Caret.Offset = result.StartOffset;
			textArea.Selection = Selection.Create(textArea, result.StartOffset, result.EndOffset);
			textArea.Caret.BringCaretToView();
			// show caret even if the editor does not have the Keyboard Focus
			textArea.Caret.Show();
		}

		void SearchLayerKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key) {
				case Key.Enter:
					e.Handled = true;
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
						FindPrevious();
					else
						FindNext();
					if (searchTextBox != null) {
						var error = Validation.GetErrors(searchTextBox).FirstOrDefault();
						if (error != null) {
							messageView.Content = Localization.ErrorText + " " + error.ErrorContent;
							messageView.PlacementTarget = searchTextBox;
							messageView.IsOpen = true;
						}
					}
					break;
				case Key.Escape:
					e.Handled = true;
					Close();
					break;
			}
		}

		public bool IsClosed { get; private set; }

		public void Close()
		{
			bool hasFocus = this.IsKeyboardFocusWithin;

			var layer = AdornerLayer.GetAdornerLayer(textArea);
			if (layer != null)
				layer.Remove(adorner);
			if (dropdownPopup != null)
				dropdownPopup.IsOpen = false;
			messageView.IsOpen = false;
			textArea.TextView.BackgroundRenderers.Remove(renderer);
			if (hasFocus)
				textArea.Focus();
			IsClosed = true;

			// Clear existing search results so that the segments don't have to be maintained
			renderer.CurrentResults.Clear();
		}

		public void Open()
		{
			if (!IsClosed) return;
			var layer = AdornerLayer.GetAdornerLayer(textArea);
			if (layer != null)
				layer.Add(adorner);
			textArea.TextView.BackgroundRenderers.Add(renderer);
			IsClosed = false;
			DoSearch(false);
		}

		public event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged;

		protected virtual void OnSearchOptionsChanged(SearchOptionsChangedEventArgs e)
		{
			if (SearchOptionsChanged != null) {
				SearchOptionsChanged(this, e);
			}
		}
	}

	public class SearchOptionsChangedEventArgs : EventArgs
	{
		public string SearchPattern { get; private set; }

		public bool MatchCase { get; private set; }

		public bool UseRegex { get; private set; }

		public bool WholeWords { get; private set; }

		public SearchOptionsChangedEventArgs(string searchPattern, bool matchCase, bool useRegex, bool wholeWords)
		{
			this.SearchPattern = searchPattern;
			this.MatchCase = matchCase;
			this.UseRegex = useRegex;
			this.WholeWords = wholeWords;
		}
	}

	class SearchPanelAdorner : Adorner
	{
		SearchPanel panel;

		public SearchPanelAdorner(TextArea textArea, SearchPanel panel)
			: base(textArea)
		{
			this.panel = panel;
			AddVisualChild(panel);
		}

		protected override int VisualChildrenCount {
			get { return 1; }
		}

		protected override Visual GetVisualChild(int index)
		{
			if (index != 0)
				throw new ArgumentOutOfRangeException();
			return panel;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			panel.Arrange(new Rect(new Point(0, 0), finalSize));
			return new Size(panel.ActualWidth, panel.ActualHeight);
		}
	}
}
