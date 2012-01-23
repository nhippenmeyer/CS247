using System;
using System.Drawing;
using System.Windows.Forms;


public delegate void CustomEventHandler();

class DelegatesAndEvents : Form
{
    // custom event handler
    public event CustomEventHandler OnLoadEvent;

    public DelegatesAndEvents()
    {
        Button clickMe = new Button();

        clickMe.Parent = this;
        clickMe.Text = "Click Me";
        clickMe.Location = new Point(
            (ClientSize.Width - clickMe.Width) / 2,
            (ClientSize.Height - clickMe.Height) / 2);        

        // C# default EventHandler delegate is assigned
        // to the button's Click event

        // += syntax registers a delegate with an event
        clickMe.Click += new EventHandler(OnClick);
        

        // our custom "EventDelegate" delegate is assigned
        // to our custom "StartEvent" event.
        OnLoadEvent += new CustomEventHandler(onLoad);

        // fire our custom event
        OnLoadEvent();
    }

    // this method is called when the "clickMe" button is pressed
    public void OnClick(object sender, EventArgs ea)
    {
        MessageBox.Show("You Clicked My Button!");
    }

   

    // this method is called when the "OnLoad" Event is fired
    public void onLoad()
    {
        MessageBox.Show("Form Loaded!");
    }

    static void Main(string[] args)
    {
        Application.Run(new DelegatesAndEvents());
    }
}