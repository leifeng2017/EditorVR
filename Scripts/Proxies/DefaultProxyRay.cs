﻿using System.Collections;
using UnityEngine;
using UnityEngine.VR.Utilities;

public class DefaultProxyRay : MonoBehaviour
{
	[SerializeField]
	private VRLineRenderer m_LineRenderer;

	[SerializeField]
	private GameObject m_Tip;

	[SerializeField]
	private float m_LineWidth;

	[SerializeField]
	private MeshFilter m_Cone;

	private enum State
	{
		Visible,
		Transitioning,
		Hidden
	}

	private State m_State;
	private Vector3 m_TipStartScale;
	private Coroutine m_Transitioning;
	private bool m_LockRay;
	private object m_LockRayObject;
	
	public void LockRay(object lockCaller)
	{
		// Mandate that a single locker caller is allowed to lock the ray
		// If the reference to the lockRayCaller is deleted, and the ray was not properly
		// unlocked by the original locking caller allow locking by another object
		if (m_LockRayObject == null)
		{
			m_LockRay = true;
			m_LockRayObject = lockCaller;
		}
	}

	public void UnlockRay(object unlockCaller)
	{
		// Only allow unlocking if the original lock caller is null or there is no locker caller set
		if (m_LockRayObject == unlockCaller || m_LockRayObject == null)
		{
			m_LockRay = false;
			m_LockRayObject = null;
		}
	}

	/// <summary>
	/// The length of the direct selection pointer
	/// </summary>
	public float pointerLength
	{
		get
		{
			return (m_Cone.transform.TransformPoint(m_Cone.sharedMesh.bounds.size.z * Vector3.forward) - m_Cone.transform.position).magnitude;
		}
	}

	public void Hide()
	{
		if (isActiveAndEnabled && m_LockRay == false)
		{
			if (m_State == State.Transitioning)
				StopAllCoroutines();
			
			StartCoroutine(HideRay());
		}
	}

	public void Show()
	{
		if (isActiveAndEnabled && m_LockRay == false)
		{
			if (m_State == State.Transitioning)
				StopAllCoroutines();
			
			StartCoroutine(ShowRay());
		}
	}

	public void SetLength(float length)
	{
		if (m_State != State.Visible)
			return;

		m_LineRenderer.transform.localScale = Vector3.one * length;
		m_LineRenderer.SetWidth(m_LineWidth, m_LineWidth*length);
		m_Tip.transform.position = transform.position + transform.forward * length;
		m_Tip.transform.localScale = length * m_TipStartScale;
	}

	private void Start()
	{
		m_TipStartScale = m_Tip.transform.localScale;
		m_State = State.Visible;
	}

	private IEnumerator HideRay()
	{
		m_State = State.Transitioning;
		m_Tip.transform.localScale = Vector3.zero;

		// cache current width for smooth animation to target value without snapping
		float currentWidth = m_LineRenderer.widthStart;
		while (currentWidth > 0)
		{
			currentWidth = U.Math.Ease(currentWidth, 0f, 3, 0.0005f);
			m_LineRenderer.SetWidth(currentWidth, currentWidth);
			yield return null;
		}

		m_LineRenderer.SetWidth(0, 0);
		m_State = State.Hidden;
	}

	private IEnumerator ShowRay()
	{
		m_State = State.Transitioning;
		m_Tip.transform.localScale = m_TipStartScale;

		float currentWidth = m_LineRenderer.widthStart;
		while (currentWidth < m_LineWidth)
		{
			currentWidth = U.Math.Ease(currentWidth, m_LineWidth, 5, 0.0005f);
			m_LineRenderer.SetWidth(currentWidth, currentWidth);

			yield return null;
		}

		// only set the value if another transition hasn't begun
		m_LineRenderer.SetWidth(m_LineWidth, m_LineWidth);
		m_State = State.Visible;
	}
}
