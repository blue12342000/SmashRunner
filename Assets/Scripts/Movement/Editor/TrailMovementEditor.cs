using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.AnimatedValues;
using System.Linq;

[CustomEditor(typeof(TrailMovement))]
[CanEditMultipleObjects]
[ExecuteAlways]
public class TrailMovementEditor : Editor
{
    TrailMovement[] m_targets;

    SerializedProperty m_collider;
    SerializedProperty m_timeScale;
    SerializedProperty m_isUseGravity;
    SerializedProperty m_gravityAcceleration;
    SerializedProperty m_collisionLayer;
    SerializedProperty m_jumpEstimate;
    SerializedProperty m_jumpDistance;
    SerializedProperty m_jumpMinAngle;
    SerializedProperty m_jumpVelocity;
    SerializedProperty m_velocity;
    SerializedProperty m_isFalling;
    SerializedProperty m_trailPath;

    bool m_IsOpenCollider = true;
    bool m_IsOpenConfig = true;
    bool m_IsOpenPhysicsGroup = true;
    GUIContent m_labelCollider = new GUIContent("- Collider");
    GUIContent m_labelExtends = new GUIContent("Extends");
    GUIContent m_labelPath = new GUIContent("* Path");
    GUIContent m_labelGravity = new GUIContent("Acceleration");
    GUIContent m_labelDistance = new GUIContent("Distance");
    GUIContent m_labelMinAngle = new GUIContent("MinAngle");
    GUIContent m_labelVelocity = new GUIContent("Velocity");
    GUIContent m_labelDebug = new GUIContent("- Debug Physics Info");
    GUIStyle m_labelStyle = new GUIStyle("Label");

    /* Simulate */
    bool m_isOnSimulate = false;
    double m_offsetTime = 0f;
    double m_elapsedTime = 0f;
    double m_prevTime = 0f;
    double m_deltaTime = 0f;
    List<IEnumerator> m_coroutines = new List<IEnumerator>();

    private void OnEnable()
    {
        m_targets = targets.Cast<TrailMovement>().ToArray();

        m_collider = serializedObject.FindProperty("m_collider");

        m_isUseGravity = serializedObject.FindProperty("m_isUseGravity");
        m_gravityAcceleration = serializedObject.FindProperty("m_gravityAcceleration");

        m_jumpEstimate = serializedObject.FindProperty("m_jumpEstimate");
        m_jumpDistance = serializedObject.FindProperty("m_jumpDistance");
        m_jumpMinAngle = serializedObject.FindProperty("m_jumpMinAngle");
        m_jumpVelocity = serializedObject.FindProperty("m_jumpVelocity");
        m_collisionLayer = serializedObject.FindProperty("m_collisionLayer");

        m_trailPath = serializedObject.FindProperty("m_trailPath");

        m_velocity = serializedObject.FindProperty("m_velocity");
        m_isFalling = serializedObject.FindProperty("m_isFalling");

        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void SimulationStart()
    {
        EditorApplication.update += OnUpdate;
        m_offsetTime = EditorApplication.timeSinceStartup - m_elapsedTime;
    }

    private void SimulationStop()
    {
        EditorApplication.update -= OnUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
        SceneView.duringSceneGui -= OnSceneGUI;
        m_prevTime = 0;
        m_elapsedTime = 0;
        m_offsetTime = 0;
        m_coroutines.Clear();
    }

    private void OnUpdate()
    {
        m_prevTime = m_elapsedTime;
        m_elapsedTime = EditorApplication.timeSinceStartup - m_offsetTime;
        m_deltaTime = m_elapsedTime - m_prevTime;
        for (int i = 0; i < m_coroutines.Count;)
        {
            if (!m_coroutines[i].MoveNext())
            {
                m_coroutines.RemoveAt(i);
            }
            else
            {
                ++i;
            }
        }
    }

    protected void OnDebugInspectorGUI()
    {
        m_IsOpenPhysicsGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_IsOpenPhysicsGroup, m_labelDebug);
        if (m_IsOpenPhysicsGroup)
        {
            EditorGUI.indentLevel++;
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_velocity);
            EditorGUILayout.PropertyField(m_isFalling);
            GUI.enabled = true;
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        m_IsOpenCollider = EditorGUILayout.BeginFoldoutHeaderGroup(m_IsOpenCollider, m_labelCollider);
        if (m_IsOpenCollider)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_collider.FindPropertyRelative("Type"));
            int colliderType = m_collider.FindPropertyRelative("Type").enumValueIndex;
            if (EditorGUI.EndChangeCheck())
            {
                switch (colliderType)
                {
                    case 0:
                    //case MovementBase.ECollider.Box:
                        m_collider.FindPropertyRelative("Param").vector3Value = Vector3.one;
                        break;
                    case 1:
                    //case MovementBase.ECollider.Capsule:
                        m_collider.FindPropertyRelative("Param").vector3Value = new Vector2(1, 0.5f);
                        break;
                    case 2:
                    //case MovementBase.ECollider.Sphere:
                        m_collider.FindPropertyRelative("Param").vector3Value = new Vector2(0.5f, 0);
                        break;
                }
            }
            if (!m_collider.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(m_collider.FindPropertyRelative("Center"));
                switch (colliderType)
                {
                    case 0:
                    //case MovementBase.ECollider.Box:
                        EditorGUILayout.PropertyField(m_collider.FindPropertyRelative("Param"), m_labelExtends);
                        break;
                    case 1:
                    //case MovementBase.ECollider.Capsule:
                        {
                            Vector3 param = m_collider.FindPropertyRelative("Param").vector3Value;
                            param.y = Mathf.Max(EditorGUILayout.FloatField("Radius", param.y), 0);
                            param.x = Mathf.Max(EditorGUILayout.FloatField("Height", param.x), param.y * 2);
                            m_collider.FindPropertyRelative("Param").vector3Value = param;
                        }
                        break;
                    case 2:
                    //case MovementBase.ECollider.Sphere:
                        {
                            Vector3 param = m_collider.FindPropertyRelative("Param").vector3Value;
                            param.x = Mathf.Max(EditorGUILayout.FloatField("Radius", param.x), 0);
                            m_collider.FindPropertyRelative("Param").vector3Value = param;
                        }
                        break;
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        m_IsOpenConfig = EditorGUILayout.BeginFoldoutHeaderGroup(m_IsOpenConfig, "- Config");
        if (m_IsOpenConfig)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_trailPath, m_labelPath);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_isUseGravity);
            GUI.enabled = m_isUseGravity.boolValue;
            EditorGUILayout.PropertyField(m_gravityAcceleration, m_labelGravity);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_collisionLayer);
            EditorGUILayout.PropertyField(m_jumpEstimate);
            if (!m_jumpEstimate.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;
                int jumpType = m_jumpEstimate.enumValueIndex;
                switch (jumpType)
                {
                    case 0:
                    //case MovementBase.EJumpEstimate.Distance:
                        EditorGUILayout.PropertyField(m_jumpMinAngle, m_labelMinAngle);
                        EditorGUILayout.PropertyField(m_jumpVelocity, m_labelVelocity);
                        break;
                    case 1:
                    //case MovementBase.EJumpEstimate.Angle:
                        EditorGUILayout.PropertyField(m_jumpDistance, m_labelDistance);
                        EditorGUILayout.PropertyField(m_jumpVelocity, m_labelVelocity);
                        break;
                    case 2:
                    //case MovementBase.EJumpEstimate.Force:
                        EditorGUILayout.PropertyField(m_jumpDistance, m_labelDistance);
                        EditorGUILayout.PropertyField(m_jumpMinAngle, m_labelMinAngle);
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        OnDebugInspectorGUI();
        serializedObject.ApplyModifiedProperties();

        if (!m_isOnSimulate)
        {
            if (GUILayout.Button("Simulate Jump", GUILayout.Height(30)))
            {
                SimulationStart();
                m_isOnSimulate = true;
                m_targets[0].CalculateEstimatedPoint(Vector3.forward * 40, 4);
            }
        }
        else
        {
            //GUILayout.BeginHorizontal();
            if (GUILayout.Button("Simulation Stop", GUILayout.Height(30)))
            {
                SimulationStop();
                m_isOnSimulate = false;
            }
            //GUILayout.EndHorizontal();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        //OnUpdate();
        Handles.color = Color.red;
        foreach (var t in m_targets)
        {
            Matrix4x4 originTRS = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(t.transform.position, t.transform.rotation, Handles.matrix.lossyScale);
            switch (t.Collider.Type)
            {
                case MovementBase.ECollider.Box:
                    {
                        Handles.DrawWireCube(t.Collider.Center, t.Collider.Param);
                    }
                    break;
                case MovementBase.ECollider.Capsule:
                    {
                        float radius = t.Collider.Param.y;
                        float halfHeight = Mathf.Max(t.Collider.Param.x - radius * 2f, 0);
                        Vector3 forward = Vector3.forward * radius;
                        Vector3 right = Vector3.right * radius;
                        Vector3 up = Vector3.up * halfHeight * 0.5f;

                        Handles.DrawWireArc(t.Collider.Center + up, Vector3.up, Vector3.right, 360, radius);
                        Handles.DrawWireArc(t.Collider.Center + up, Vector3.forward, Vector3.right, 180, radius);
                        Handles.DrawWireArc(t.Collider.Center + up, -Vector3.right, Vector3.forward, 180, radius);

                        Handles.DrawLine(t.Collider.Center + forward + up, t.Collider.Center + forward - up);
                        Handles.DrawLine(t.Collider.Center - (forward + up), t.Collider.Center - (forward - up));
                        Handles.DrawLine(t.Collider.Center + right + up, t.Collider.Center + right - up);
                        Handles.DrawLine(t.Collider.Center - (right + up), t.Collider.Center - (right - up));

                        Handles.DrawWireArc(t.Collider.Center - up, Vector3.down, Vector3.right, 360, radius);
                        Handles.DrawWireArc(t.Collider.Center - up, -Vector3.forward, Vector3.right, 180, radius);
                        Handles.DrawWireArc(t.Collider.Center - up, Vector3.right, Vector3.forward, 180, radius);
                    }
                    break;
                case MovementBase.ECollider.Sphere:
                    {
                        Handles.DrawWireArc(t.Collider.Center, Vector3.up, Vector3.forward, 360, t.Collider.Param.x);
                        Handles.DrawWireArc(t.Collider.Center, Vector3.forward, Vector3.up, 360, t.Collider.Param.x);
                    }
                    break;
            }
            Handles.matrix = originTRS;
        }

        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(10, 10, 200, 60), EditorStyles.textArea);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("Elapsed Time:");
        m_labelStyle.alignment = TextAnchor.MiddleRight;
        GUILayout.Label($"{m_elapsedTime.ToString("0.000")}", m_labelStyle);
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label("delta Time:");
        m_labelStyle.alignment = TextAnchor.MiddleRight;
        GUILayout.Label($"{m_deltaTime.ToString("0.000")}", m_labelStyle);
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
        Handles.EndGUI();

        sceneView.Repaint();
    }

    private IEnumerator Simulation(TrailMovement target)
    {
        yield return null;
    }
}
