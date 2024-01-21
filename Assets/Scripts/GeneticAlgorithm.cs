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
    // 1���゠����̌̐�
    public int IndividualsCount { get; set; } = 100;

    // �̌Q
    public List<Individual<T>> Individuals { get; set; } = new();

    // ��������ڂ�
    public int GenerationIndex { get; set; } = 0;
    public float MaxFitness { get; set; } = -0.0f;
    public float AverageFitness { get; set; } = -0.0f;

    // ��ꐢ��̌̌Q���擾
    public virtual List<Individual<T>> GetFirstGenerationIndividuals()
    {
        return new List<Individual<T>>();
    }

    //�̌Q����e�ƂȂ�2�̂�I�o(�����I��)
    public virtual Individual<T>[] GetParentIndividuals(List<Individual<T>> individuals)
    {
        return new Individual<T>[]
        {
            individuals[UnityEngine.Random.Range(0, individuals.Count)],
            individuals[UnityEngine.Random.Range(0, individuals.Count)]
        };
    }

    //�e��2�̂���q��2�̂𐶐�
    public virtual Individual<T>[] GetChildrenIndividuals(Individual<T>[] parentIndividuals)
    {
        return new Individual<T>[]
        {
            parentIndividuals[UnityEngine.Random.Range(0, parentIndividuals.Length)],
            parentIndividuals[UnityEngine.Random.Range(0, parentIndividuals.Length)]
        };
    }

    //����̂��ˑR�ψق����̂𐶐�
    public virtual Individual<T> GetMutatedIndividual(Individual<T> individual)
    {
        return individual;
    }

    //�e2�́E�q2�́E����ȊO�̌̂̒��Ő�������̂�I��(�����I��)
    public virtual List<Individual<T>> GetSurvivingIndividuals(
        List<Individual<T>> individuals,
        Individual<T>[] parentIndividuals,
        Individual<T>[] childrenIndividuals)
    {
        return individuals;
    }

    //�T���̏I������
    public virtual bool EndSearchCondition()
    {
        return false;
    }

    //���K���x���擾���邽�߂̍s��
    public virtual async Task Action()
    {

    }

    //���K���x���擾���邽�߂̍s��
    public virtual async Task ActionForChildren(Individual<T>[] children)
    {

    }

    //��`�I�A���S���Y���ɂ��T�����s��
    public virtual async Task Search()
    {
        Debug.Log("�T���J�n!");

        GenerationIndex = 0;
        Individuals = GetFirstGenerationIndividuals();

        while (true)
        {
            //���㐔���J�E���g�A�b�v
            GenerationIndex++;

            //���K���x���擾���邽�߂̍s�����s��
            await Action();

            //�̌Q�̊��K���x���Z�o
            foreach (Individual<T> individual in Individuals)
            {
                if (individual.FitnessFunc != null)
                {
                    individual.Fitness = individual.FitnessFunc.Invoke(Individuals, individual);
                }
            }

            #region ���O�o�́E�ő�K���̐ݒ�
            // if (GenerationIndex % 2 == 1)
            {
                Debug.Log($"�T����({GenerationIndex}�����)");

                // ���K���x�̍~���Ń\�[�g
                List<float> sortedFitnesses = Individuals
                .Map(individual => individual.Fitness)
                .OrderByDescending(fitness => fitness).ToList();

                if (sortedFitnesses.Count >= 1)
                {
                    // �ő�K���x
                    MaxFitness = sortedFitnesses[0];

                    // �K���x����
                    AverageFitness = sortedFitnesses
                    .Average();

                    // �K���x���U
                    float variance = sortedFitnesses
                    .Variance();

                    Debug.Log($"�ő���K���x:{MaxFitness},���ϊ��K���x:{AverageFitness}, ���K���x���U:{variance}");
                }
            }
            #endregion

            if (EndSearchCondition())
            {
                BestIndividual = Individuals.OrderByDescending(Individual => Individual.Fitness).ToList()[0];
                Debug.Log("�T���I���!");
                break;
            }

            //�e�ƂȂ�2�̂�I��(�����I��)
            Individual<T>[] parentIndividuals = GetParentIndividuals(Individuals);

            //�e2�̂���q2�̂𐶐�
            Individual<T>[] childrenIndividuals = GetChildrenIndividuals(parentIndividuals);

            //�������ꂽ�q2�̂ɓˑR�ψق�K�p
            for (int i = 0; i < childrenIndividuals.Length; i++)
            {
                childrenIndividuals[i] = GetMutatedIndividual(childrenIndividuals[i]);
            }

            //���K���x���擾���邽�߂̍s�����s��
            await ActionForChildren(childrenIndividuals);

            //�������ꂽ�q2�̂̊��K���x���Z�o
            foreach (Individual<T> individual in childrenIndividuals)
            {
                if (individual.FitnessFunc != null)
                {
                    individual.Fitness = individual.FitnessFunc.Invoke(childrenIndividuals.ToList(), individual);
                }
            }

            //��������̂�I��(�����I��)���āA�V���Ȍ̌Q�ōČv�Z
            Individuals = GetSurvivingIndividuals(Individuals, parentIndividuals, childrenIndividuals);
        }

        Debug.Log("�T���I��!");
    }
}