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

    // 第一世代の個体群を取得
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

    //個体群から親となる2個体を選出(複製選択)→遺伝子の異なる2個体をランダム非復元抽出
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

    //親の2個体から子の2個体を生成→遺伝子を半分ずつ交換
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

    //ある個体が突然変異した個体を生成
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

    //親2個体・子2個体・それ以外の個体の中で生存する個体を選択(生存選択)
    public override List<Individual<int>> GetSurvivingIndividuals(
        List<Individual<int>> individuals,
        Individual<int>[] parentIndividuals,
        Individual<int>[] childrenIndividuals)
    {
        List<Individual<int>> survivingIndividuals = new List<Individual<int>>();

        //親個体を個体群から一旦除く
        foreach (Individual<int> parentIndividual in parentIndividuals)
        {
            while (individuals.Contains(parentIndividual))
            {
                individuals.Remove(parentIndividual);
            }
        }

        //親でも子でもない個体は全員生存
        foreach (Individual<int> individual in individuals)
        {
            survivingIndividuals.Add(individual);
        }

        //親と子からなる家族個体群
        List<Individual<int>> familyIndividuals = new List<Individual<int>>();

        foreach (Individual<int> parentIndividual in parentIndividuals)
        {
            familyIndividuals.Add(parentIndividual);
        }

        foreach (Individual<int> childIndividual in childrenIndividuals)
        {
            familyIndividuals.Add(childIndividual);
        }

        //環境適合度が高い順にソート
        familyIndividuals = familyIndividuals.OrderByDescending(f => f.Fitness).ToList();

        //最も適合度の高い家族個体を生存させる
        if (familyIndividuals.Count >= 1)
        {
            Individual<int> bestFamilyIndividual = familyIndividuals[0];

            familyIndividuals.Remove(bestFamilyIndividual);

            survivingIndividuals.Add(bestFamilyIndividual);
        }

        //残りの家族個体からルーレット戦略で生存個体を選出
        if (familyIndividuals.Count >= 1)
        {
            //環境適合度の絶対値の逆数の合計
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

    //探索の終了条件
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

    //環境適合度を取得するための行動
    public override async Task Action()
    {
        if (ActionTask != null)
        {
            await ActionTask();
        }
    }

    //環境適合度を取得するための行動
    public override async Task ActionForChildren(Individual<int>[] children)
    {
        if (ActionTaskForChildren != null)
        {
            await ActionTaskForChildren(children);
        }
    }

    //遺伝的アルゴリズムによる探索を行う
    public override async Task Search()
    {
        await base.Search();
    }
}