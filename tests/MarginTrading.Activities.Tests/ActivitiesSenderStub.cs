using System;
using System.Collections.Generic;
using System.Linq;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.Services.Abstractions;

namespace MarginTrading.Activites.Tests;

public class ActivitiesSenderStub : IActivitiesSender
{
    private List<IActivity> _activities = new List<IActivity>();
    
    public IReadOnlyList<IActivity> Activities => _activities;

    public void PublishActivity(IActivity activity)
    {
        _activities.Add(activity);
    }
    
    public IActivity GetLastPublishedActivity()
    {
        if(_activities.Count == 0)
            throw new InvalidOperationException("could not found any activity");
        
        return _activities.LastOrDefault();
    }
}