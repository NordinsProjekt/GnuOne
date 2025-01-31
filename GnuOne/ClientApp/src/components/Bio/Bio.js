﻿import { useState, useContext, useEffect } from "react"
import PortContext from '../../contexts/portContext';
import MeContext from '../../contexts/meContext';
import FriendContext from '../../contexts/friendContext';
import './bio.css'

const Bio = () => {
    const { friendEmail } = useContext(FriendContext)
    const myEmail = useContext(MeContext)
    const port = useContext(PortContext)
    const url = `https://localhost:${port}/api/`
    const [profile, setProfile] = useState()
    const [tagList, setTagList] = useState()


    useEffect(() => {
        async function fetchData() {
            console.log(friendEmail, myEmail)

            if (friendEmail === undefined) {
                //get my own profile
                const response = await fetch(url + 'myprofile')
                const me = await response.json()
                console.log(me[0])
                setProfile(me[0])

            } else {
                //get my friend's profile
                const responseOne = await fetch(url + 'myfriends/' + friendEmail)
                const friend = await responseOne.json()
                console.log(friend.MyFriend)
                setProfile(friend.MyFriend)

            }
        }
        fetchData()

    }, [myEmail, setProfile, url, friendEmail])

    useEffect(() => {
        async function getTags() {
            if (profile) {
                console.log(profile)
                const response = await fetch(url + 'tags')
                const tags = await response.json()
                console.log(tags)
                console.log(profile.tagOne)
                let userTags = tags.filter(tag => tag.ID === profile.tagOne || tag.ID === profile.tagTwo || tag.ID === profile.tagThree)
                console.log(userTags)
                setTagList(userTags)
            }
        }
        getTags()
    }, [profile, setTagList, url])


    return (

        <section className="bio-container">

            {profile ? <><h3> about me </h3>
                <p> {profile.userInfo || profile.myUserInfo}</p>

                <h3> my interests </h3>
                <div>
                    {tagList && tagList.length > 0 ? <> {tagList[0]?.tagName}  ✨ {tagList[1]?.tagName} ✨ {tagList[2]?.tagName}</> : null}
                </div>

                <h3> public key </h3>
                <p> {profile.pubKey || profile.Secret}</p>

                <h3> contact me </h3>
                <p>{profile.Email}</p></>
                : "loading..."}


        </section>

    )
}

export default Bio