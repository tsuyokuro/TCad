//#define DEFAULT_DATA_TYPE_DOUBLE
using Plotter;
using Plotter.Controller;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad.ViewModel;

public class LayerListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged(String info)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    public ObservableCollection<LayerHolder> LayerList_ = new ObservableCollection<LayerHolder>();
    public ObservableCollection<LayerHolder> LayerList
    {
        get
        {
            return LayerList_;
        }
        set
        {
            LayerList_ = value;
            NotifyPropertyChanged("LayerList");
        }
    }

    public LayerHolder SelectedItem
    {
        get
        {
            int idx = GetLayerListIndex(mContext.Controller.CurrentLayer.ID);
            if (idx < 0)
            {
                return null;
            }
            return LayerList[idx];
        }

        set
        {
            LayerHolder lh = value;
            if (lh == null)
            {
                return;
            }

            if (mContext.Controller.CurrentLayer.ID != lh.ID)
            {
                mContext.Controller.SetCurrentLayer(lh.ID);

                mContext.Redraw();
            }

            NotifyPropertyChanged("SelectedItem");
        }
    }

    private IPlotterViewModel mContext;

    public LayerListViewModel(IPlotterViewModel context)
    {
        mContext = context;
    }

    public void LayerListItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        LayerHolder lh = (LayerHolder)sender;
        mContext.Redraw();
    }

    public void LayerListChanged(LayerListInfo layerListInfo)
    {
        foreach (LayerHolder lh in LayerList)
        {
            lh.PropertyChanged -= LayerListItemPropertyChanged;
        }

        LayerList.Clear();

        foreach (CadLayer layer in layerListInfo.LayerList)
        {
            LayerHolder layerHolder = new LayerHolder(layer);
            layerHolder.PropertyChanged += LayerListItemPropertyChanged;

            LayerList.Add(layerHolder);
        }

        int idx = GetLayerListIndex(layerListInfo.CurrentID);
        if (idx >= 0)
        {
            SelectedItem = LayerList[idx];
        }
    }

    private int GetLayerListIndex(uint id)
    {
        int idx = 0;
        foreach (LayerHolder layer in LayerList)
        {
            if (layer.ID == id)
            {
                return idx;
            }
            idx++;
        }

        return -1;
    }
}
