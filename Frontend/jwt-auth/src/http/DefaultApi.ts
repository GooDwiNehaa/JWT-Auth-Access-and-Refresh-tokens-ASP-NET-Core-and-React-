import axios from 'axios'

const defaultApi = axios.create({
    withCredentials: true,
    baseURL: process.env.REACT_APP_SERVER_URL,
    headers: {
        'Content-Type': 'application/json',
        Accept: '*/*',
        'Content-Encoding': 'gzip, deflate, br',
    },
})

export default defaultApi
