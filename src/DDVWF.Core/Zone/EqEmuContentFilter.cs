namespace DDVWF.Core.Zone;

public static class EqEmuContentFilter
{
    public static bool Passes(int currentExpansion,IReadOnlySet<string> enabledFlags,IReadOnlySet<string> disabledFlags,int minExpansion,int maxExpansion,string contentFlags,string contentFlagsDisabled)
    {
        if(minExpansion>-1&&currentExpansion<minExpansion&&currentExpansion!=-1)return false;
        if(maxExpansion>-1&&currentExpansion>maxExpansion&&currentExpansion!=-1)return false;
        foreach(var flag in contentFlags.Split(',',StringSplitOptions.RemoveEmptyEntries))if(!enabledFlags.Contains(flag))return false;
        foreach(var flag in contentFlagsDisabled.Split(',',StringSplitOptions.RemoveEmptyEntries))if(!disabledFlags.Contains(flag))return false;
        return true;
    }
}
