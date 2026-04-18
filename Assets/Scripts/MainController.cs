using System.Collections;
using System.Collections.Generic;
using GuanYao.Tool.Singleton;
using UnityEngine;

public class MainController : SingletonMono<MainController>
{
    public List<GameObject> MenuList;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CloseMenuUi()
    {
        foreach (var menu in MenuList)
        {
            menu.SetActive(false);
        }
    }
    
    public void LoadMenu(int index)
    {
        CloseMenuUi();
        MenuList[index].SetActive(true);
    }
        
}
