﻿new Vue({
    el: '#vue-comment',
    data: {
        comments: [],
        total: 0,
        page: 1,
        pageSize: 5,
        search: '',
        sortBy: '',
        refreshLoading: true,
        submitLoading: false,
        emailDisabled: false,
        emailOtpDisabled: false,
        getEmailOtpDisabled: false,
        getEmailOtpLoading: false,
        userNameDisabled: true,
        userNameLoading: false,
        urlDisabled: true,
        urlLoading: false,
        contentDisabled: true,
        submitDisabled: true,
        replyDisabled: true,
        replyComment: null,
        getEmailOtpText: 'Get',
        otpInterval: null,
        form: {
            parentId: '',
            postId: POST_ID,
            userName: '',
            email: '',
            emailOtp: '',
            url: '',
            content: '',
        },
        formRules: {
            userName: [
                {required: true, message: 'Please enter a username', trigger: 'blur'},
                {min: 2, max: 20, message: 'Length must be between 2 and 20 characters', trigger: 'blur'}
            ],
            email: [
                {required: true, message: 'Please enter an email address', trigger: 'blur'},
                {type: 'email', message: 'Invalid email format'}
            ],
            emailOtp: [
                {required: true, message: 'Please enter the email verification code', trigger: 'change'},
                {len: 4, message: 'Length must be 4 characters', trigger: 'change'}
            ],
            url: [
                {type: 'url', message: 'Please enter a valid URL', trigger: 'blur'},
            ],
            content: [
                {required: true, message: 'Please enter the comment content', trigger: 'blur'},
                {min: 1, max: 300, message: 'Length must be between 1 and 300 characters', trigger: 'blur'},
                // {whitespace: true, message: 'Comment content cannot be only whitespace', trigger: 'blur'},
            ]
        },
    },
    created: async function () {
        await this.getComments()
        this.refreshLoading = false
    },
    methods: {
        getEmailOtp(email) {
            return new Promise((resolve, reject) => {
                axios.get(`/Api/Comment/GetEmailOtp?email=${email}`)
                    .then(res => resolve(res.data))
                    .catch(res => resolve(res.response.data))
            })
        },
        getAnonymousUser(email, otp) {
            return new Promise((resolve, reject) => {
                axios.get(`/Api/Comment/GetAnonymousUser?email=${email}&otp=${otp}`)
                    .then(res => resolve(res.data))
                    .catch(res => resolve(res.response.data))
            })
        },
        getComments() {
            let params = {
                PostId: POST_ID,
                Page: this.page,
                PageSize: this.pageSize,
            }
            if (this.search) params.Search = this.search
            if (this.sortBy) params.SortBy = this.sortBy

            return new Promise((resolve, reject) => {
                axios.get(`/Api/Comment`, {params})
                    .then(res => {
                        this.comments = res.data.data.map(e => {
                            let item = {
                                ...e,
                                anonymousUser: {
                                    ...e.anonymousUser,
                                    createdTime: dayjs(e.anonymousUser.createdTime).format('YYYY-MM-DD'),
                                    updatedTime: dayjs(e.anonymousUser.updatedTime).format('YYYY-MM-DD'),
                                },
                                createdTime: dayjs(e.createdTime).format('YYYY-MM-DD'),
                                updatedTime: dayjs(e.updatedTime).format('YYYY-MM-DD'),
                                avatar: `/Api/PicLib/Random/100/100?Seed=${e.anonymousUserId}`,
                            }

                            if (e.parent) {
                                item.replyUser = e.parent.anonymousUser.name
                            }

                            return item
                        })
                        this.total = res.data.pagination.totalItemCount
                        resolve(res)
                    })
                    .catch(res => {
                        console.log(res)
                        let data = res.response.data
                        resolve(data)
                    })
            })
        },
        submitComment(data) {
            return new Promise((resolve, reject) => {
                axios.post(`/Api/Comment`, {...data})
                    .then(res => resolve(res.data))
                    .catch(res => resolve(res.response.data))
            })
        },
        async handleRefresh() {
            this.refreshLoading = true
            await this.getComments()
            this.refreshLoading = false
        },
        async handleGetEmailOtp() {
            if (!this.form.email) {
                this.$message.error('Please enter an email address!');
                return;
            }
            this.getEmailOtpLoading = true
            let res = await this.getEmailOtp(this.form.email)
            this.getEmailOtpLoading = false
            if (res.successful) {
                this.$message.success(res.message)
                // Sent successfully, show countdown
                let countdown = 60 * 5
                this.getEmailOtpText = `${countdown}s`
                this.otpInterval = setInterval(() => {
                    countdown--
                    if (countdown > 0) {
                        this.getEmailOtpText = `${countdown}s`
                        this.getEmailOtpDisabled = true
                    } else {
                        countdown = 60 * 5
                        this.getEmailOtpText = 'Get'
                        if (this.otpInterval) clearInterval(this.otpInterval)
                        this.getEmailOtpDisabled = false
                    }
                }, 1000)
            } else {
                this.$message.error(res.message)
            }
        },
        async handleEmailOtpChange(value) {
            console.log('handleEmailOtpChange', value)
            if (this.form.email?.length === 0 || value.length < 4) return

            this.userNameLoading = true
            this.urlLoading = true
            let res = await this.getAnonymousUser(this.form.email, value)
            console.log(res)
            if (res.successful) {
                if (res.data.anonymousUser) {
                    this.form.userName = res.data.anonymousUser.name
                    this.form.url = res.data.anonymousUser.url
                }
                this.form.emailOtp = res.data.newOtp
                // Lock the email and verification code fields, no need to edit them
                this.getEmailOtpDisabled = true
                this.emailDisabled = true
                this.emailOtpDisabled = true
                // Enable editing of username, URL, content, and reply
                this.userNameDisabled = false
                this.urlDisabled = false
                this.contentDisabled = false
                this.submitDisabled = false
                this.replyDisabled = false
            } else {
                this.$message.error(res.message)
            }
            this.userNameLoading = false
            this.urlLoading = false
        },
        handleUrlBlur() {
            if (!this.form.url.startsWith('http'))
                this.form.url = `http://${this.form.url}`
        },
        handleReset() {
            this.$refs.form.resetFields()
            if (this.otpInterval) clearInterval(this.otpInterval)
            this.getEmailOtpText = 'Get'
            this.emailDisabled = false
            this.emailOtpDisabled = false
            this.getEmailOtpDisabled = false
            this.userNameDisabled = true
            this.urlDisabled = true
            this.contentDisabled = true
            this.submitDisabled = true
            this.replyDisabled = true
            this.form.parentId = ''
            this.replyComment = null
        },
        async handleSubmit() {
            this.$refs.form.validate(async (valid) => {
                if (valid) {
                    this.submitLoading = true
                    let res = await this.submitComment(this.form)
                    if (res.successful) {
                        this.$message.success(res.message)
                        let email = `${this.form.email}`
                        this.handleReset()
                        this.form.email = email
                    } else this.$message.error(res.message)
                    this.submitLoading = false
                    await this.getComments()
                }
            })
        },
        async handleSizeChange(value) {
            this.pageSize = value
            await this.handleRefresh()
        },
        async handleCurrentChange(value) {
            this.page = value
            await this.handleRefresh()
        },
        async handleReply(comment) {
            this.replyComment = comment
            this.form.parentId = comment.id
            this.$refs.content.focus()
        },
        handleReplyTagClose() {
            this.form.parentId = ''
            this.replyComment = null
        },
    }
})