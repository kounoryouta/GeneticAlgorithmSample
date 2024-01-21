using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Unity.VisualScripting.Dependencies.Sqlite;

public class GeneticAlgorithm<T>
{
    public Individual<T> BestIndividual { get; set; } = null;
    // 1世代あたりの個体数
    public int IndividualsCount { get; set; } = 100;

    // 個体群
    public List<Individual<T>> Individuals { get; set; } = new();

    // 今何世代目か
    public int GenerationIndex { get; set; } = 0;
    public float MaxFitness { get; set; } = -0.0f;
    public float AverageFitness { get; set; } = -0.0f;

    // 第一世代の個体群を取得
    public virtual List<Individual<T>> GetFirstGenerationIndividuals()
    {
        return new List<Individual<T>>();
    }

    //個体群から親となる2個体を選出(複製選択)
    public virtual Individual<T>[] GetParentIndividuals(List<Individual<T>> individuals)
    {
        return new Individual<T>[]
        {
            individuals[UnityEngine.Random.Range(0, individuals.Count)],
            individuals[UnityEngine.Random.Range(0, individuals.Count)]
        };
    }

    //親の2個体から子の2個体を生成
    public virtual Individual<T>[] GetChildrenIndividuals(Individual<T>[] parentIndividuals)
    {
        return new Individual<T>[]
        {
            parentIndividuals[UnityEngine.Random.Range(0, parentIndividuals.Length)],
            parentIndividuals[UnityEngine.Random.Range(0, parentIndividuals.Length)]
        };
    }

    //ある個体が突然変異した個体を生成
    public virtual Individual<T> GetMutatedIndividual(Individual<T> individual)
    {
        return individual;
    }

    //親2個体・子2個体・それ以外の個体の中で生存する個体を選択(生存選択)
    public virtual List<Individual<T>> GetSurvivingIndividuals(
        List<Individual<T>> individuals,
        Individual<T>[] parentIndividuals,
        Individual<T>[] childrenIndividuals)
    {
        return individuals;
    }

    //探索の終了条件
    public virtual bool EndSearchCondition()
    {
        return false;
    }

    //環境適合度を取得するための行動
    public virtual async Task Action()
    {

    }

    //環境適合度を取得するための行動
    public virtual async Task ActionForChildren(Individual<T>[] children)
    {

    }

    //遺伝的アルゴリズムによる探索を行う
    public virtual async Task Search()
    {
        Debug.Log("探索開始!");

        GenerationIndex = 0;
        Individuals = GetFirstGenerationIndividuals();

        while (true)
        {
            //世代数をカウントアップ
            GenerationIndex++;

            //環境適合度を取得するための行動を行う
            await Action();

            //個体群の環境適合度を算出
            foreach (Individual<T> individual in Individuals)
            {
                if (individual.FitnessFunc != null)
                {
                    individual.Fitness = individual.FitnessFunc.Invoke(Individuals, individual);
                }
            }

            #region ログ出力・最大適合個体設定
            // if (GenerationIndex % 2 == 1)
            {
                Debug.Log($"探索中({GenerationIndex}世代目)");

                // 環境適合度の降順でソート
                List<float> sortedFitnesses = Individuals
                .Map(individual => individual.Fitness)
                .OrderByDescending(fitness => fitness).ToList();

                if (sortedFitnesses.Count >= 1)
                {
                    // 最大適合度
                    MaxFitness = sortedFitnesses[0];

                    // 適合度平均
                    AverageFitness = sortedFitnesses
                    .Average();

                    // 適合度分散
                    float variance = sortedFitnesses
                    .Variance();

                    Debug.Log($"最大環境適合度:{MaxFitness},平均環境適合度:{AverageFitness}, 環境適合度分散:{variance}");
                }
            }
            #endregion

            if (EndSearchCondition())
            {
                BestIndividual = Individuals.OrderByDescending(Individual => Individual.Fitness).ToList()[0];
                Debug.Log("探索終わり!");
                break;
            }

            //親となる2個体を選択(複製選択)
            Individual<T>[] parentIndividuals = GetParentIndividuals(Individuals);

            //親2個体から子2個体を生成
            Individual<T>[] childrenIndividuals = GetChildrenIndividuals(parentIndividuals);

            //生成された子2個体に突然変異を適用
            for (int i = 0; i < childrenIndividuals.Length; i++)
            {
                childrenIndividuals[i] = GetMutatedIndividual(childrenIndividuals[i]);
            }

            //環境適合度を取得するための行動を行う
            await ActionForChildren(childrenIndividuals);

            //生成された子2個体の環境適合度を算出
            foreach (Individual<T> individual in childrenIndividuals)
            {
                if (individual.FitnessFunc != null)
                {
                    individual.Fitness = individual.FitnessFunc.Invoke(childrenIndividuals.ToList(), individual);
                }
            }

            //生存する個体を選択(生存選択)して、新たな個体群で再計算
            Individuals = GetSurvivingIndividuals(Individuals, parentIndividuals, childrenIndividuals);
        }

        Debug.Log("探索終了!");
    }
}