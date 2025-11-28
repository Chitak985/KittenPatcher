using Brutal.Logging;
using KSA;
using KSA.Rendering.Water.Data;
using System.Runtime.InteropServices;

namespace Kitten_Patcher
{
  [StarMapMod]
  public class MainKittenPatcher
  {
    public var currentCSystem;
    public List<Astronomical> astronomicalList;

    [StarMapAllModsLoaded]
    public void OnFullyLoaded()
    {
      currentCSystem = KSA.Universe.CelestialSystem;
      astronomicalList = currentCSystem.All.GetList();
      Console.WriteLine(astronomicalList[0].Id);
      Astronomical(currentCSystem, astronomicalList[0].bodyTemplate, "(Patched)"+astronomicalList[0].Id);
    }
  }
}
