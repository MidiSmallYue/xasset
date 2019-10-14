using UnityEditor;
using UnityEngine;
using Plugins.XAsset;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// ֧�ִ������ݻ��ƣ�bundle����Դ̫������
/// </summary>
public class XAssetDebugView : EditorWindow
{
    private static LoopView bundleLoopView;
    private static LoopView assetLoopView;
    public static List<string> toggleBundles;

    private static int oldBundleCount;
    private static int oldAssetBundleCount;

    [MenuItem("Tool/XAsset RefCount")]
    private static void Init()
    {
        var re = new Rect(0, 0, 1000, 700);
        var window = GetWindowWithRect<XAssetDebugView>(re);
        bundleLoopView = new LoopView();
        assetLoopView = new LoopView();
        bundleLoopView.viewPosX = 0;
        assetLoopView.viewPosX = window.position.width / 2;
        bundleLoopView.toggles = new bool[Bundles.bundles.Count];
        //assetLoopView.toggles = new bool[40000];
        bundleLoopView.s_RowCount = Bundles.bundles.Count;
        assetLoopView.s_RowCount = Assets.bundleAssets.Count;
        oldBundleCount = Bundles.bundles.Count;
        oldAssetBundleCount = Assets.bundleAssets.Count;
        bundleLoopView.isBundleView = true;
        toggleBundles = new List<string>();
        window.titleContent = new GUIContent("XAsset���ò鿴");
        window.Show();
    }

    private const int s_RowCount = 40000;

    private float m_RowBundleHeight = 18f;
    private float m_ColBundleWidth = 52f;
    private Vector2 m_ScrollPosition;

    private const int s_RowAssetCount = 40000;

    private float m_RowAssetHeight = 18f;
    private float m_ColAssetWidth = 52f;
    private Vector2 assetPos;

    void OnGUI()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isCompiling && bundleLoopView != null)
        {
            if (oldBundleCount != Bundles.bundles.Count)
            {
                bundleLoopView.s_RowCount = Bundles.bundles.Count;
                oldBundleCount = Bundles.bundles.Count;
                bundleLoopView.toggles = new bool[Bundles.bundles.Count];
            }
            if (oldAssetBundleCount != Assets.bundleAssets.Count)
            {
                assetLoopView.s_RowCount = Assets.bundleAssets.Count;
                oldAssetBundleCount = Assets.bundleAssets.Count;
            }
            bundleLoopView.OnDraw(position);
            assetLoopView.OnDraw(position);
        }

        //OnDrawBundle();
        //OnDrawAsset();
    }

    private void OnDrawBundle()
    {
        Rect totalRect = new Rect(position.width / 2, 0, position.width / 2, position.height);
        Rect contentRect = new Rect(0, 0, m_ColBundleWidth, s_RowCount * m_RowBundleHeight);
        m_ScrollPosition = GUI.BeginScrollView(totalRect, m_ScrollPosition, contentRect);

        int num;
        int num2;
        GetBundleFirstAndLastRowVisible(out num, out num2, totalRect.height);
        if (num2 >= 0)
        {
            int numVisibleRows = num2 - num + 1;
            DrawBundle(num, numVisibleRows, contentRect.width, totalRect.height);
        }

        GUI.EndScrollView(true);
    }

    /// <summary>
    /// ��ȡ����ʾ����ʼ�кͽ�����
    /// </summary>
    /// <param name="firstRowVisible">��ʼ��</param>
    /// <param name="lastRowVisible">������</param>
    /// <param name="viewHeight">��ͼ�߶�</param>
    private void GetBundleFirstAndLastRowVisible(out int firstRowVisible, out int lastRowVisible, float viewHeight)
    {
        if (s_RowCount == 0)
        {
            firstRowVisible = lastRowVisible = -1;
        }
        else
        {
            float y = m_ScrollPosition.y;
            float height = viewHeight;
            firstRowVisible = (int)Mathf.Floor(y / m_RowBundleHeight);
            lastRowVisible = firstRowVisible + (int)Mathf.Ceil(height / m_RowBundleHeight);
            firstRowVisible = Mathf.Max(firstRowVisible, 0);
            lastRowVisible = Mathf.Min(lastRowVisible, s_RowCount - 1);
            if (firstRowVisible >= s_RowCount && firstRowVisible > 0)
            {
                m_ScrollPosition.y = 0f;
                GetBundleFirstAndLastRowVisible(out firstRowVisible, out lastRowVisible, viewHeight);
            }
        }
    }

    /// <summary>
    /// �������ƿ���ʾ����
    /// </summary>
    /// <param name="firstRow">��ʼ��</param>
    /// <param name="numVisibleRows">�ܿ���ʾ����</param>
    /// <param name="rowWidth">ÿ�еĿ��</param>
    /// <param name="viewHeight">��ͼ�߶�</param>
    private void DrawBundle(int firstRow, int numVisibleRows, float rowWidth, float viewHeight)
    {
        int i = 0;
        while (i < numVisibleRows)
        {
            int num2 = firstRow + i;
            Rect rowRect = new Rect(0f, (float)num2 * m_RowBundleHeight, rowWidth, m_RowBundleHeight);
            float num3 = rowRect.y - m_ScrollPosition.y;
            if (num3 <= viewHeight)
            {
                Rect colRect = new Rect(rowRect);
                colRect.width = m_ColBundleWidth;

                GUI.Button(colRect, "btn");
                colRect.x += 100;
                GUI.Label(new Rect(colRect), num2.ToString());
            }
            i++;
        }
    }



    private void OnDrawAsset()
    {
        Rect totalRect = new Rect(position.width / 2, 0, position.width / 2, position.height);
        Rect contentRect = new Rect(0, 0, m_ColAssetWidth, s_RowAssetCount * m_RowAssetHeight);
        m_ScrollPosition = GUI.BeginScrollView(totalRect, m_ScrollPosition, contentRect);

        int num;
        int num2;
        GetBundleFirstAndLastRowVisible(out num, out num2, totalRect.height);
        if (num2 >= 0)
        {
            int numVisibleRows = num2 - num + 1;
            DrawBundle(num, numVisibleRows, contentRect.width, totalRect.height);
        }

        GUI.EndScrollView(true);
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}

public class LoopView
{
    public int s_RowCount = 40000;

    public float m_RowHeight = 18f;
    public float m_ColWidth = 300;
    public Vector2 m_ScrollPosition;

    public float viewPosX;

    public bool[] toggles;

    public bool isBundleView;

    private List<string> assetNames;

    private List<string> activeAssets;

    public void OnDraw(Rect position)
    {
        Rect totalRect = new Rect(viewPosX, 0, position.width / 2, position.height);
        Rect contentRect = new Rect(0, 0, m_ColWidth, s_RowCount * m_RowHeight);
        m_ScrollPosition = GUI.BeginScrollView(totalRect, m_ScrollPosition, contentRect);

        int num;
        int num2;
        GetFirstAndLastRowVisible(out num, out num2, totalRect.height);
        if (num2 >= 0)
        {
            int numVisibleRows = num2 - num + 1;
            Draw(num, numVisibleRows, contentRect.width, totalRect.height);
        }

        GUI.EndScrollView(true);
    }

    /// <summary>
    /// ��ȡ����ʾ����ʼ�кͽ�����
    /// </summary>
    /// <param name="firstRowVisible">��ʼ��</param>
    /// <param name="lastRowVisible">������</param>
    /// <param name="viewHeight">��ͼ�߶�</param>
    private void GetFirstAndLastRowVisible(out int firstRowVisible, out int lastRowVisible, float viewHeight)
    {
        if (s_RowCount == 0)
        {
            firstRowVisible = lastRowVisible = -1;
        }
        else
        {
            float y = m_ScrollPosition.y;
            float height = viewHeight;
            firstRowVisible = (int)Mathf.Floor(y / m_RowHeight);
            lastRowVisible = firstRowVisible + (int)Mathf.Ceil(height / m_RowHeight);
            firstRowVisible = Mathf.Max(firstRowVisible, 0);
            lastRowVisible = Mathf.Min(lastRowVisible, s_RowCount - 1);
            if (firstRowVisible >= s_RowCount && firstRowVisible > 0)
            {
                m_ScrollPosition.y = 0f;
                GetFirstAndLastRowVisible(out firstRowVisible, out lastRowVisible, viewHeight);
            }
        }
    }

    /// <summary>
    /// �������ƿ���ʾ����
    /// </summary>
    /// <param name="firstRow">��ʼ��</param>
    /// <param name="numVisibleRows">�ܿ���ʾ����</param>
    /// <param name="rowWidth">ÿ�еĿ��</param>
    /// <param name="viewHeight">��ͼ�߶�</param>
    private void Draw(int firstRow, int numVisibleRows, float rowWidth, float viewHeight)
    {
        if (!isBundleView)
        {
            assetNames = Assets.bundleAssets.Keys.ToList();
            activeAssets = Assets.assets.Select(asset => asset.name).ToList(); ;
        }
        int i = 0;
        XAssetDebugView.toggleBundles.Clear();
        while (i < numVisibleRows)
        {
            int num2 = firstRow + i;
            Rect rowRect = new Rect(0, (float)num2 * m_RowHeight, rowWidth, m_RowHeight);
            float num3 = rowRect.y - m_ScrollPosition.y;
            if (num3 <= viewHeight)
            {
                Rect colRect = new Rect(rowRect);
                colRect.width = m_ColWidth;

                colRect.x += 20;
                if (isBundleView)
                {
                    if (Bundles.bundles.Count > 0)
                    {
                        var bundleName = Bundles.bundles[num2].name;
                        toggles[num2] = GUI.Toggle(colRect, toggles[num2], "");
                        if (toggles[num2])
                        {
                            XAssetDebugView.toggleBundles.Add(bundleName);
                        }
                        var index = bundleName.LastIndexOf("AssetBundles");
                        var tempName = bundleName.Substring(index, bundleName.Length - index);
                        colRect.x += 50;
                        GUI.Label(colRect, tempName);
                        colRect.x += 350;
                        GUI.Label(colRect, Bundles.bundles[num2].refCount.ToString());
                    }
                }
                else
                {
                    if (XAssetDebugView.toggleBundles.Count > 0)
                    {
                        //��ʾ��ѡ��bundle ��asset�ļ�
                        var asset = assetNames[num2];
                        string bundle;
                        if (Assets.GetAssetBundleName(asset, out bundle))
                        {
                            if (XAssetDebugView.toggleBundles.Contains(bundle))
                            {
                                var assetName = assetNames[num2];
                                var refcount = 0;
                                if (activeAssets.Contains(assetName))
                                {
                                    GUI.color = Color.red;
                                    var tempAsset = Assets.assets.Find(
                                            a => a.name == assetName);
                                    refcount = tempAsset.refCount;
                                }
                                else
                                {
                                    GUI.color = Color.gray;
                                }
                                GUI.Label(colRect, assetName);
                                colRect.x += 300;
                                GUI.Label(colRect, refcount.ToString());
                            }
                        }
                    }
                    else
                    {
                        //û��toggle ��ѡ����ʾ����asset
                        var assetName = assetNames[num2];
                        var refcount = 0;
                        if (activeAssets.Contains(assetName))
                        {
                            GUI.color = Color.red;
                            var asset = Assets.assets.Find(
                                a => a.name == assetName);
                            refcount = asset.refCount;
                        }
                        else
                        {
                            GUI.color = Color.gray;
                        }
                        GUI.Label(colRect, assetName);
                        colRect.x += 300;
                        GUI.Label(colRect, refcount.ToString());
                    }

                }
            }
            i++;
        }
    }

}