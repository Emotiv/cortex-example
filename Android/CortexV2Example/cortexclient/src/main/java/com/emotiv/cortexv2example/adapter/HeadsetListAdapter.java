package com.emotiv.cortexv2example.adapter;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;
import com.emotiv.cortexv2example.R;
import com.emotiv.cortexv2example.objects.HeadsetObject;
import java.util.List;

public class HeadsetListAdapter extends ArrayAdapter<HeadsetObject> {
    private Context context;
    private List<HeadsetObject> headsets;

    public HeadsetListAdapter(Context context, List<HeadsetObject> headsets) {
        super(context, 0, headsets);
        this.context = context;
        this.headsets = headsets;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        if (convertView == null) {
            LayoutInflater inflater = LayoutInflater.from(context);
            convertView = inflater.inflate(R.layout.headset_list_item, parent, false);
        }
        TextView tvHeadsetName = (TextView) convertView.findViewById(R.id.tvHeadsetName);
        tvHeadsetName.setText(headsets.get(position).getHeadsetName());

        TextView tvHeadsetConnectedBy = (TextView) convertView.findViewById(R.id.tvHeadsetConnectedBy);
        tvHeadsetConnectedBy.setText(headsets.get(position).getHeadsetConnectedBy());

        TextView tvHeadsetStatus = (TextView) convertView.findViewById(R.id.tvHeadsetStatus);
        tvHeadsetStatus.setText(headsets.get(position).getHeadsetStatus());

        return convertView;
    }
}
