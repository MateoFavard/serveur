module Page.MyAccount exposing (Model, Msg, init, session, update, view)

import Browser exposing (Document)
import Html exposing (Html)
import Html.Attributes as Attr

import Session exposing (..)
import Route exposing (Route, MyAccountFragment(..))


type alias Model = Session


type alias Msg = ()


init : Session -> MyAccountFragment -> ( Model, Cmd Msg )
init oldSession fragment =
  ( oldSession, Cmd.none )


session : Model -> Session
session model = model


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
  ( model, Cmd.none )


view : Model -> Html Msg
view model =
  Html.main_ [ ]
    [ Html.div [ Attr.class "box" ]
      [ Html.h1 [ ] [ Html.text "Mon compte" ]
      , Html.p [ ] [ Html.text "Cette fonctionnalité n'est pas encore disponible" ]
      ]
    ]