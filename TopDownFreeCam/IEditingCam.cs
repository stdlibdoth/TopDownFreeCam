using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEditCamMove<T>
{
    public void SetCenter(T posistion);
    public void MoveUpdate(T data);
    public void MoveEnd();
    public void MoveStart();
}

public interface IEditCamRotate<T>
{
    public void Rotate(T step);
    public void SetRotation(T angle);
}

public interface IEditCamZoom<T>
{
    public void Zoom(T step, MinMax range);
    public void SetZoom(T zoom_value);
    public void ZoomRange(MinMax range);
}

public interface IEditCamPitch<T>
{
    public void Pitch(T step, MinMax range);
    public void SetPitch(T pitch_angle);
}