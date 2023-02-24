using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;

public interface ISampleManager : ISingleton
{

}

public class SampleManager : Singleton<UnitManager, ISampleManager>, ISampleManager
{
}
