using System;
using System.Collections.Generic;
using System.Windows;

namespace TCad.Controls
{
    public interface IAutoCompleteTextBox
    {
        List<string> CandidateList { get; }

        event Action<string> Determined;

        void Enter();
    }
}
