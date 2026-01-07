using Godot;
using System;
using Game.Player.Upgrades;

public partial class UpgradeMenu : Node {
	public int exp;
	
	public UpgradeManager upManRef;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		upManRef = GetParent<UpgradeManager>();
		HookButtons();
		exp = 5; //for now
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		HookLabels();
	}

	void HookLabels() {
		Label healthLabel =  GetNode<Label>( "%HealthLabel" );
		healthLabel.Text = "Currently max health is level " + upManRef.GetUpgradeTier(UpgradeType.MaxHealth);
		Label defenseLabel = GetNode<Label>( "%DefenseLabel" );
		defenseLabel.Text = "Currently armor is level " + upManRef.GetUpgradeTier(UpgradeType.Armor);
		Label speedLabel = GetNode<Label>( "%SpeedLabel" );
		speedLabel.Text = "Currently speed is level " + upManRef.GetUpgradeTier(UpgradeType.Speed);
		Label attackLabel = GetNode<Label>("%AttackLabel");
		attackLabel.Text = "Currently attack power is level " + upManRef.GetUpgradeTier(UpgradeType.AttackDamage);
		Label attackSpeedLabel = GetNode<Label>("%AttackSpeedLabel");
		attackSpeedLabel.Text = "currently attack speed is level " + upManRef.GetUpgradeTier(UpgradeType.AttackSpeed);
		Label expLabel = GetNode<Label>("%ExpLabel");
		expLabel.Text = "Upgrade Bucks Available: " + exp;
	}
	
	private void HookButtons() {
		Button healthButton = GetNode<Button>("%UpgradeHealth");
		healthButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPress(UpgradeType.MaxHealth)));
		Button speedButton = GetNode<Button>("%UpgradeSpeed");
		speedButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPress(UpgradeType.Speed)));
		Button defenseButton = GetNode<Button>("%UpgradeDefense");
		defenseButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPress(UpgradeType.Armor)));
		Button attackButton = GetNode<Button>("%UpgradeAttack");
		attackButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPress(UpgradeType.AttackDamage)));
		Button attackSpeedButton = GetNode<Button>("%UpgradeAttackSpeed");
		attackButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPress(UpgradeType.AttackSpeed)));
	}
	public void OnButtonPress(UpgradeType type) {
		bool isUpgradeOwned = upManRef.UpgradeIsOwned(type);

		if(exp > 0 && !isUpgradeOwned) {
			exp--;
			upManRef.BuyUpgrade(type);
		} else if(isUpgradeOwned) {
			GD.Print("You already have that upgrade");
		} else {
			GD.Print("Sorry but you don't have enough exp!");
		}
	}
}
