using System.Linq.Expressions;
using WorkflowCore.Interface;
using WorkflowCore.Primitives;

namespace VacationBot.Steps;

public static class StepBuilderExtensions
{
    public static IStepBuilder<TData, SendMessage> SendMessage<TData>(
        this IWorkflowBuilder<TData> builder, Func<TData, string> userIdFunc, string message)
    {
        return builder
            .StartWith<SendMessage>()
            .Input(step => step.UserId, data => userIdFunc(data))
            .Input(step => step.Message, _ => message);
    }

    public static IStepBuilder<TData, SendMessage> SendMessage<TData>(
        this IWorkflowBuilder<TData> builder, Func<TData, string> userIdFunc, Func<TData, string> messageFunc)
    {
        return builder
            .StartWith<SendMessage>()
            .Input(step => step.UserId, data => userIdFunc(data))
            .Input(step => step.Message, data => messageFunc(data));
    }

    public static IStepBuilder<TData, SendMessage> SendMessage<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, Func<TData, string> userIdFunc, string message)
        where TStep : IStepBody
    {
        return builder
            .Then<SendMessage>()
            .Input(step => step.UserId, data => userIdFunc(data))
            .Input(step => step.Message, _ => message);
    }

    public static IStepBuilder<TData, SendMessage> SendMessage<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, Func<TData, string> userIdFunc, Func<TData, string> messageFunc)
        where TStep : IStepBody
    {
        return builder
            .Then<SendMessage>()
            .Input(step => step.UserId, data => userIdFunc(data))
            .Input(step => step.Message, data => messageFunc(data));
    }

    public static IStepBuilder<TData, SendVariants> SendVariants<TData, TStep>(
        this IStepBuilder<TData, TStep> builder, Func<TData, string> userIdFunc, string message, string[] variants)
        where TStep : IStepBody
    {
        return builder
            .Then<SendVariants>()
            .Input(step => step.UserId, data => userIdFunc(data))
            .Input(step => step.Message, _ => message)
            .Input(step => step.Variants, _ => variants);
    }

    public static IStepBuilder<TData, WaitFor> WaitForUserMessage<TData, TStep, TOutput>(
        this IStepBuilder<TData, TStep> builder, Func<TData, string> userIdFunc, Expression<Func<TData, TOutput>> dataProperty)
        where TStep : IStepBody
    {
        return builder
            .WaitFor("UserMessage", data => userIdFunc(data), _ => DateTime.UtcNow)
            .Output(dataProperty, step => step.EventData);
    }

    public static IStepBuilder<TData, WaitFor> WaitForUserMessage<TData, TStep, TOutput>(
        this IStepBuilder<TData, TStep> builder, Func<TData, string> userIdFunc, Expression<Func<TData, TOutput>> dataProperty, Func<object, TOutput> dataConverter)
        where TStep : IStepBody
    {
        return builder
            .WaitFor("UserMessage", data => userIdFunc(data), _ => DateTime.UtcNow)
            .Output(dataProperty, step => dataConverter(step.EventData));
    }
}