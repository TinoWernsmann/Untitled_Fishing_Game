


using UnityEngine;

public class MaterialHeightReaderTask
{
    private string name = "";
    public Vector3 pos;
    public Vector3 result;

    private bool enableDownOffset = true;

    public bool bEnableDownOffset()
    {
        return enableDownOffset;
    }



    public MaterialHeightReaderTask()
    {

    }

    public MaterialHeightReaderTask(Vector3 posIn)
    {
        Setup(posIn);
    }
    
    public MaterialHeightReaderTask(Vector3 posIn, string s)
    {
        Setup(posIn, s);
    }



    void Setup(Vector3 posIn)
    {
        pos = posIn;
    }

    void Setup(Vector3 posin, string nameIn)
    {
        Setup(posin);
        name = nameIn;
    }


    public string GetName()
    {
        return name;
    }



};