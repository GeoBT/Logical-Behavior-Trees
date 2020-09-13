﻿using System;

public class BlackboardElementTypeAttribute : Attribute
{
    public Type ElementType { get; private set; }
    public BlackboardElementTypeAttribute(Type type)
    {
        ElementType = type;
    }
}