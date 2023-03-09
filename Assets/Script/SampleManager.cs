using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public interface ISampleManager : ISingleton
{

}

public class SampleManager : Singleton<SampleManager, ISampleManager>, ISampleManager
{

}
