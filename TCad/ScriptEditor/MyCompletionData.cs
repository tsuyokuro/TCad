using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Windows.Media;

namespace TCad.ScriptEditor
{
    public class MyCompletionData : ICompletionData
    {
        //入力候補一覧に表示される内容
        public object Content
        {
            get
            {
                return Text;
            }
            set
            {
            }
        }

        public object Description { get; set; }

        // Item icon
        public ImageSource Image { get; set; }

        public double Priority { get; set; }

        public string Text { get; set; }

        public WordData mWordData;

        public MyCompletionData(string text, WordData wd)
        {
            Text = text;
            mWordData = wd;
        }

        //アイテム選択後の処理
        public void Complete(
            TextArea textArea,
            ISegment completionSegment,
            EventArgs insertionRequestEventArgs
            )
        {
            //textArea.Document.Replace(completionSegment, Text);

            textArea.Document.Replace(mWordData.StartPos, mWordData.Word.Length, Text);
        }
    }
}

