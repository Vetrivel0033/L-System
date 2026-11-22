using System;

[Serializable]
public class LSystemRule
{
    public string symbol;      
    public string replacement;  
}


[Serializable]
public class LSystemData
{
    public string name;             
    public string axiom;           
    public LSystemRule[] rules;    
    public int iterations = 4;     
    public float angle = 25f;       
    public float stepLength = 1f;  
}
