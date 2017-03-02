#pragma strict

var fog = true;
var fogColor = Color(0, 0.4, 0.7, 0.6);
var fogDensity = 0.6;
var skybox : Material;

function Start () {
    RenderSettings.fog = fog;
    RenderSettings.fogColor = fogColor;
    RenderSettings.fogDensity = fogDensity;
    RenderSettings.skybox = skybox;
}

function Update () {

}