using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// WebGL-safe way to await
/// </summary>
public static class SafeTask
{
	public static async Task Delay(float seconds)
	{
		var timeStart = Time.fixedTime;
		while (Time.fixedTime < timeStart + seconds && Application.isPlaying)
			await Task.Yield();
	}
	public static async Task Delay(float seconds, CancellationToken token)
	{
		var timeStart = Time.fixedTime;
		while (Time.fixedTime < timeStart + seconds 
		       && !token.IsCancellationRequested
		       && Application.isPlaying)
			await Task.Yield();
	}

	public static async Task SafeWaitForCompletion(this Task task)
	{
		while (!task.IsCompleted
		       && Application.isPlaying)
			await Task.Yield();
	}

	public static async Task<T> SafeWaitForCompletion<T>(this Task<T> task, CancellationToken token)
	{
		while (!task.IsCompleted && !token.IsCancellationRequested && Application.isPlaying)
			await Task.Yield();
		return task.Result;
	}

	public static async Task Await(Func<bool> result, CancellationToken token)
	{
		while (!result.Invoke() && Application.isPlaying && !token.IsCancellationRequested)
			await Task.Yield();
	}

	public static async Task Await(Func<bool> result)
	{
		while (!result.Invoke() && Application.isPlaying)
			await Task.Yield();
	}
}