﻿using ScreenshotsMod.Framework.UserModels;

namespace ScreenshotsMod.Framework.ModModels;

/// <summary>
/// Represents a processed trigger condition.
/// </summary>
/// <param name="Day">The packed day.</param>
/// <param name="Times">The allowed times.</param>
/// <param name="Weather">The weather allowed.</param>
internal readonly record struct ProcessedTrigger(PackedDay Day, TimeRange[] Times, Weather Weather, uint Delay)
{
    /// <summary>
    /// Checks to see if this processed trigger is valid.
    /// </summary>
    /// <param name="current">The current location.</param>
    /// <param name="currentDay">The (packed) current day.</param>
    /// <param name="timeOfDay">The current time of day.</param>
    /// <returns>Whether or not this is valid.</returns>
    internal bool Check(GameLocation current, PackedDay currentDay, int timeOfDay, uint lastTriggeredDiff)
    {
        if (lastTriggeredDiff < this.Delay)
        {
            return false;
        }

        // check weather.
        switch (this.Weather)
        {
            case Weather.Sunny when current.IsRainingHere():
                return false;
            case Weather.Rainy when !current.IsRainingHere():
                return false;
        }

        // check to see if my day is valid. The first four bits are my season, other twenty eight are the days.
        if (!this.Day.Check(currentDay))
        {
            return false;
        }

        // check to see if I'm in a valid time range
        foreach (TimeRange range in this.Times)
        {
            if (range.StartTime <= timeOfDay && range.EndTime >= timeOfDay)
            {
                return true;
            }
        }
        return false;
    }
}
