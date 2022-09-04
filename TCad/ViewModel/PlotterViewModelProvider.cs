using Microsoft.Scripting.Actions.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TCad.ViewModel
{
    public class PlotterViewModelProvider
    {
        private static PlotterViewModelProvider sInstance = new PlotterViewModelProvider();

        private PlotterViewModel mPlotterViewModel;

        public static PlotterViewModelProvider Instance {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return sInstance;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public PlotterViewModel Get()
        {
            return mPlotterViewModel;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Set(PlotterViewModel vm)
        {
            mPlotterViewModel = vm;
        }
    }
}
