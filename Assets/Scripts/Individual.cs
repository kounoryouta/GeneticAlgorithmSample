using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// 1ŒÂ‘Ì
public class Individual<T>
{
    public Individual(int generationIndex, T[] chromosome, Func<List<Individual<T>>, Individual<T>, float> fitnessFunc)
    {
        GenerationIndex = generationIndex;
        Chromosome = chromosome.CloneArray();
        FitnessFunc = fitnessFunc;
    }
    public float Fitness { get; set; } = 0f;
    public int GenerationIndex { get; private set; } = 0;
    public T[] Chromosome { get; private set; } = new T[0];
    public Func<List<Individual<T>>, Individual<T>, float> FitnessFunc { get; private set; } = null;
}