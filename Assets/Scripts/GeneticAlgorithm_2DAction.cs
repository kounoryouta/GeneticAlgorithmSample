using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.XR;

public class GeneticAlgorithm_2DAction : GeneticAlgorithm<int>
{
    public Func<Task> ActionTask { get; set; } = null;
    public Func<Individual<int>[], Task> ActionTaskForChildren { get; set; } = null;
    public Func<List<Individual<int>>, Individual<int>, float> FitnessFunc { get; set; } = null;
    public int ActionIDCount { get; set; } = 0;
    public float MutePossiblility { get; set; } = 0.2f;
    public int MaxGeneration { get; set; } = 10;

    // ��ꐢ��̌̌Q���擾
    public override List<Individual<int>> GetFirstGenerationIndividuals()
    {
        List<Individual<int>> individuals = new List<Individual<int>>();

        for (int i = 0; i < IndividualsCount; i++)
        {
            int[] chromosome = new int[ActionIDCount];

            for (int j = 0; j < ActionIDCount; j++)
            {
                chromosome[j] = UnityEngine.Random.Range(0, 3);
            }

            individuals.Add(new Individual<int>(
                generationIndex: GenerationIndex,
                chromosome: chromosome,
                fitnessFunc: FitnessFunc
            ));
        }

        return individuals;
    }

    //�̌Q����e�ƂȂ�2�̂�I�o(�����I��)����`�q�̈قȂ�2�̂������_���񕜌����o
    public override Individual<int>[] GetParentIndividuals(List<Individual<int>> individuals)
    {
        Individual<int>[] parentIndividuals = new Individual<int>[2];

        for (int i = 0; i < 2; i++)
        {
            do
            {
                parentIndividuals[i] = individuals[UnityEngine.Random.Range(0, individuals.Count)];
            }

            while (i != 0 && parentIndividuals[0].Chromosome.SequenceEqual(parentIndividuals[1].Chromosome));
        }

        return parentIndividuals;
    }

    //�e��2�̂���q��2�̂𐶐�����`�q�𔼕�������
    public override Individual<int>[] GetChildrenIndividuals(Individual<int>[] parentIndividuals)
    {
        Individual<int>[] childrenIndividuals = new Individual<int>[2];

        for (int i = 0; i < 2; i++)
        {
            int[] chromosome = new int[parentIndividuals[0].Chromosome.Length];

            int border = UnityEngine.Random.Range(0, parentIndividuals[0].Chromosome.Length);

            for (int j = 0; j < parentIndividuals[0].Chromosome.Length; j++)
            {
                if (i == 0)
                {
                    if (j < border)
                    {
                        chromosome[j] = parentIndividuals[0].Chromosome[j];
                    }

                    else
                    {
                        chromosome[j] = parentIndividuals[1].Chromosome[j];
                    }
                }

                else if (i == 1)
                {
                    if (j < border)
                    {
                        chromosome[j] = parentIndividuals[1].Chromosome[j];
                    }

                    else
                    {
                        chromosome[j] = parentIndividuals[0].Chromosome[j];
                    }
                }
            }

            Individual<int> childIndividual = new Individual<int>(
            generationIndex: parentIndividuals[0].GenerationIndex + 1,
            chromosome: chromosome,
            fitnessFunc: FitnessFunc);

            childrenIndividuals[i] = childIndividual;
        }

        return childrenIndividuals;
    }

    //����̂��ˑR�ψق����̂𐶐�
    public override Individual<int> GetMutatedIndividual(Individual<int> individual)
    {
        Individual<int> mutatedIndividual = new Individual<int>(
            generationIndex: individual.GenerationIndex,
            chromosome: individual.Chromosome,
            fitnessFunc: FitnessFunc);

        for (int i = 0; i < mutatedIndividual.Chromosome.Length; i++)
        {
            if (RandomUtility.IsSucceedProbability(MutePossiblility))
            {
                mutatedIndividual.Chromosome[i] = UnityEngine.Random.Range(0, 3);
            }
        }

        return mutatedIndividual;
    }

    //�e2�́E�q2�́E����ȊO�̌̂̒��Ő�������̂�I��(�����I��)
    public override List<Individual<int>> GetSurvivingIndividuals(
        List<Individual<int>> individuals,
        Individual<int>[] parentIndividuals,
        Individual<int>[] childrenIndividuals)
    {
        List<Individual<int>> survivingIndividuals = new List<Individual<int>>();

        //�e�̂��̌Q�����U����
        foreach (Individual<int> parentIndividual in parentIndividuals)
        {
            while (individuals.Contains(parentIndividual))
            {
                individuals.Remove(parentIndividual);
            }
        }

        //�e�ł��q�ł��Ȃ��̂͑S������
        foreach (Individual<int> individual in individuals)
        {
            survivingIndividuals.Add(individual);
        }

        //�e�Ǝq����Ȃ�Ƒ��̌Q
        List<Individual<int>> familyIndividuals = new List<Individual<int>>();

        foreach (Individual<int> parentIndividual in parentIndividuals)
        {
            familyIndividuals.Add(parentIndividual);
        }

        foreach (Individual<int> childIndividual in childrenIndividuals)
        {
            familyIndividuals.Add(childIndividual);
        }

        //���K���x���������Ƀ\�[�g
        familyIndividuals = familyIndividuals.OrderByDescending(f => f.Fitness).ToList();

        //�ł��K���x�̍����Ƒ��̂𐶑�������
        if (familyIndividuals.Count >= 1)
        {
            Individual<int> bestFamilyIndividual = familyIndividuals[0];

            familyIndividuals.Remove(bestFamilyIndividual);

            survivingIndividuals.Add(bestFamilyIndividual);
        }

        //�c��̉Ƒ��̂��烋�[���b�g�헪�Ő����̂�I�o
        if (familyIndividuals.Count >= 1)
        {
            //���K���x�̐�Βl�̋t���̍��v
            float sum = familyIndividuals.Map(individual => individual.Fitness).Sum();

            if (sum == 0f)
            {
                sum = 0.01f;
            }

            Individual<int> survivingIndividual = null;

            do
            {
                survivingIndividual = familyIndividuals[UnityEngine.Random.Range(0, familyIndividuals.Count)];
            }

            while (ContinueCondition());

            bool ContinueCondition()
            {
                if (survivingIndividual == null)
                {
                    return true;
                }

                float possibility = survivingIndividual.Fitness / sum;

                if (!RandomUtility.IsSucceedProbability(possibility))
                {
                    return true;
                }

                return false;
            }

            survivingIndividuals.Add(survivingIndividual);
        }

        return survivingIndividuals;
    }

    //�T���̏I������
    public override bool EndSearchCondition()
    {
        if (GenerationIndex >= 2)
        {
            if (MaxFitness >= 100000)
            {
                return true;
            }
        }

        if (GenerationIndex >= MaxGeneration)
        {
            return true;
        }

        return false;
    }

    //���K���x���擾���邽�߂̍s��
    public override async Task Action()
    {
        if (ActionTask != null)
        {
            await ActionTask();
        }
    }

    //���K���x���擾���邽�߂̍s��
    public override async Task ActionForChildren(Individual<int>[] children)
    {
        if (ActionTaskForChildren != null)
        {
            await ActionTaskForChildren(children);
        }
    }

    //��`�I�A���S���Y���ɂ��T�����s��
    public override async Task Search()
    {
        await base.Search();
    }
}